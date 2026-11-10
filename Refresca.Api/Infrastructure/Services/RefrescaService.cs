using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Refresca.Api.Application.DTOs;
using Refresca.Api.Application.Interfaces;
using Refresca.Api.Domain.Entities;
using Refresca.Api.Infrastructure.Persistence;

namespace Refresca.Api.Infrastructure.Services;

public sealed class RefrescaService(
    RefrescaDbContext db,
    IRepository<Leitura> leituras,
    IRepository<PausaHidratacao> pausas,
    IRepository<ClimaDiario> climas,
    IUnitOfWork unitOfWork,
    IPrevisaoClient previsao,
    TimeProvider tempo) : IRefrescaService
{
    private const string MensagemPerigo =
        "Hoje o calor deve ficar perigoso. Leve água, faça pausas na sombra e beba água mesmo sem sede.";
    private const string MensagemNormal =
        "Sem previsão de calor perigoso para hoje. Mesmo assim, mantenha a garrafa de água por perto.";

    public async Task<LeituraResponseDto> RegistrarLeituraAsync(RegistrarLeituraDto dto, CancellationToken ct)
    {
        var pontoExiste = await db.PontosRefresca.AnyAsync(p => p.TotemId == dto.TotemId, ct);
        if (!pontoExiste) throw new KeyNotFoundException($"Totem '{dto.TotemId}' não está cadastrado.");

        var nivelCalculado = Leitura.ClassificarNivel(dto.IndiceCalor);
        if (!string.Equals(dto.Nivel, nivelCalculado, StringComparison.Ordinal))
            throw new ArgumentException($"Nível informado incompatível. Para índice {dto.IndiceCalor}, o nível esperado é '{nivelCalculado}'.");

        var leitura = new Leitura(dto.TotemId, dto.Temperatura, dto.Umidade, dto.IndiceCalor, dto.MedidoEm);
        await leituras.AddAsync(leitura, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(leitura);
    }

    public async Task<IReadOnlyList<PontoResponseDto>> ListarPontosAsync(CancellationToken ct)
    {
        var pontos = await db.PontosRefresca.AsNoTracking().OrderBy(p => p.Id).ToListAsync(ct);
        var totens = pontos.Select(p => p.TotemId).ToList();

        // Leituras mais recentes dos totens cadastrados; a última de cada totem é escolhida em memória.
        var recentes = await db.Leituras.AsNoTracking()
            .Where(l => totens.Contains(l.TotemId))
            .OrderByDescending(l => l.MedidoEm)
            .Take(500)
            .ToListAsync(ct);
        var ultimaPorTotem = recentes
            .GroupBy(l => l.TotemId)
            .ToDictionary(g => g.Key, g => g.First());

        return pontos
            .Select(p => ToPontoDto(p, ultimaPorTotem.TryGetValue(p.TotemId, out var l) ? ToUltimaDto(l) : null))
            .ToList();
    }

    public async Task<PontoResponseDto?> ObterPontoAsync(int id, CancellationToken ct)
    {
        var ponto = await db.PontosRefresca.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (ponto is null) return null;

        var ultima = await db.Leituras.AsNoTracking()
            .Where(l => l.TotemId == ponto.TotemId)
            .OrderByDescending(l => l.MedidoEm)
            .FirstOrDefaultAsync(ct);

        return ToPontoDto(ponto, ultima is null ? null : ToUltimaDto(ultima));
    }

    public async Task<PausaResponseDto> RegistrarPausaAsync(RegistrarPausaDto dto, CancellationToken ct)
    {
        if (dto.PontoRefrescaId.HasValue &&
            !await db.PontosRefresca.AnyAsync(p => p.Id == dto.PontoRefrescaId.Value, ct))
            throw new KeyNotFoundException("Ponto Refresca não encontrado.");

        var pausa = new PausaHidratacao(dto.TrabalhadorId, dto.PontoRefrescaId,
            dto.RegistradaEm, dto.DuracaoMin, dto.BebeuAgua);
        await pausas.AddAsync(pausa, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ToPausaDto(pausa);
    }

    public async Task<IReadOnlyList<PausaResponseDto>> ListarPausasAsync(int trabalhadorId, CancellationToken ct)
    {
        var lista = await db.PausasHidratacao.AsNoTracking()
            .Where(p => p.TrabalhadorId == trabalhadorId)
            .OrderByDescending(p => p.RegistradaEm)
            .ToListAsync(ct);
        return lista.Select(ToPausaDto).ToList();
    }

    public async Task<AlertaHojeDto> ObterAlertaHojeAsync(CancellationToken ct)
    {
        var hoje = RefrescaRelogio.HojeEmSaoPaulo(tempo);
        var dataTexto = hoje.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        // 1º caminho: modelo de ML, com os 3 dias anteriores a hoje.
        var inicio = hoje.AddDays(-3);
        var dias = await db.ClimasDiarios.AsNoTracking()
            .Where(c => c.Dia >= inicio && c.Dia < hoje)
            .OrderBy(c => c.Dia)
            .ToListAsync(ct);

        if (dias.Count == 3)
        {
            var requisicao = new PrevisaoRequisicaoDto(dataTexto, dias.Select(ToClimaDto).ToList());
            var resultado = await previsao.PreverAsync(requisicao, ct);
            if (resultado is not null)
            {
                return new AlertaHojeDto(dataTexto, resultado.Perigoso, Math.Round(resultado.Probabilidade, 4),
                    resultado.Perigoso ? MensagemPerigo : MensagemNormal,
                    $"Serviço de previsão ({resultado.VersaoModelo})");
            }
        }

        // 2º caminho: sem serviço de previsão ou sem os 3 dias, usa a última leitura dos totens.
        var ultima = await db.Leituras.AsNoTracking()
            .OrderByDescending(l => l.MedidoEm)
            .FirstOrDefaultAsync(ct);

        if (ultima is null)
            return new AlertaHojeDto(dataTexto, false, null,
                "Ainda não há dados para gerar o alerta de hoje.", "Sem previsão nem leituras");

        var mensagem = ultima.Nivel switch
        {
            "perigo" => MensagemPerigo,
            "atencao" => "Atenção ao calor: aumente a hidratação e programe pausas em locais com sombra.",
            _ => MensagemNormal
        };
        return new AlertaHojeDto(dataTexto, ultima.Nivel == "perigo", null, mensagem,
            "Última leitura dos totens (serviço de previsão indisponível)");
    }

    public async Task<ClimaDiarioDto> RegistrarClimaDiarioAsync(RegistrarClimaDiarioDto dto, CancellationToken ct)
    {
        var dia = dto.Dia.Date;
        var existente = await db.ClimasDiarios.FirstOrDefaultAsync(c => c.Dia == dia, ct);
        if (existente is null)
        {
            existente = new ClimaDiario(dia, dto.TempMax, dto.TempMin, dto.TempMedia, dto.UmidMedia, dto.UmidMin,
                dto.IndiceCalorMax, dto.PressaoMedia, dto.RadiacaoTotal, dto.PrecipTotal, dto.VentoMedio);
            await climas.AddAsync(existente, ct);
        }
        else
        {
            existente.Atualizar(dto.TempMax, dto.TempMin, dto.TempMedia, dto.UmidMedia, dto.UmidMin,
                dto.IndiceCalorMax, dto.PressaoMedia, dto.RadiacaoTotal, dto.PrecipTotal, dto.VentoMedio);
        }
        await unitOfWork.SaveChangesAsync(ct);
        return ToClimaDto(existente);
    }

    public async Task<IReadOnlyList<ClimaDiarioDto>> ListarClimaDiarioAsync(int dias, CancellationToken ct)
    {
        var lista = await db.ClimasDiarios.AsNoTracking()
            .OrderByDescending(c => c.Dia)
            .Take(Math.Clamp(dias, 1, 90))
            .ToListAsync(ct);
        return lista.Select(ToClimaDto).ToList();
    }

    private static LeituraResponseDto ToDto(Leitura l) =>
        new(l.Id, l.TotemId, l.Temperatura, l.Umidade, l.IndiceCalor, l.Nivel, l.MedidoEm);

    private static UltimaLeituraDto ToUltimaDto(Leitura l) => new(l.IndiceCalor, l.Nivel, l.MedidoEm);

    private static PontoResponseDto ToPontoDto(PontoRefresca p, UltimaLeituraDto? ultima) =>
        new(p.Id, p.Nome, p.Endereco, p.Latitude, p.Longitude, p.TemAgua, p.TemSombra, p.TemBanheiro,
            p.Horario, ultima);

    private static PausaResponseDto ToPausaDto(PausaHidratacao p) =>
        new(p.Id, p.TrabalhadorId, p.PontoRefrescaId, p.RegistradaEm, p.DuracaoMin, p.BebeuAgua);

    private static ClimaDiarioDto ToClimaDto(ClimaDiario c) =>
        new(c.Dia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), c.TempMax, c.TempMin, c.TempMedia,
            c.UmidMedia, c.UmidMin, c.IndiceCalorMax, c.PressaoMedia, c.RadiacaoTotal, c.PrecipTotal, c.VentoMedio);
}
