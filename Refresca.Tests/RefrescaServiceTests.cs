using Microsoft.EntityFrameworkCore;
using Refresca.Api.Application.DTOs;
using Refresca.Api.Application.Interfaces;
using Refresca.Api.Domain.Entities;
using Refresca.Api.Infrastructure.Persistence;
using Refresca.Api.Infrastructure.Repositories;
using Refresca.Api.Infrastructure.Services;
using Xunit;

namespace Refresca.Tests;

public class RefrescaServiceTests
{
    // 12h do dia 20/11/2026 em São Paulo
    private static readonly TimeProvider Tempo = new TempoFixo(new DateTimeOffset(2026, 11, 20, 15, 0, 0, TimeSpan.Zero));

    private sealed class TempoFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }

    private sealed class PrevisaoFalsa(PrevisaoResultadoDto? resposta) : IPrevisaoClient
    {
        public PrevisaoRequisicaoDto? Recebida { get; private set; }

        public Task<PrevisaoResultadoDto?> PreverAsync(PrevisaoRequisicaoDto requisicao, CancellationToken ct)
        {
            Recebida = requisicao;
            return Task.FromResult(resposta);
        }
    }

    private static async Task<(RefrescaService Service, RefrescaDbContext Db)> CriarAsync(IPrevisaoClient previsao)
    {
        var opcoes = new DbContextOptionsBuilder<RefrescaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new RefrescaDbContext(opcoes);
        await SeedData.InitializeAsync(db, Tempo);
        var service = new RefrescaService(db, new Repository<Leitura>(db), new Repository<PausaHidratacao>(db),
            new Repository<ClimaDiario>(db), new UnitOfWork(db), previsao, Tempo);
        return (service, db);
    }

    [Fact]
    public async Task Alerta_DeveUsarOServicoDePrevisaoComOsTresDiasAnteriores()
    {
        var previsao = new PrevisaoFalsa(new PrevisaoResultadoDto(1, "2026-11-20", 0.70428, true, "xgboost-1.0"));
        var (service, _) = await CriarAsync(previsao);

        var alerta = await service.ObterAlertaHojeAsync(CancellationToken.None);

        Assert.Equal("2026-11-20", alerta.Data);
        Assert.True(alerta.Perigoso);
        Assert.Equal(0.7043, alerta.Probabilidade);
        Assert.Contains("xgboost-1.0", alerta.Fonte);
        Assert.NotNull(previsao.Recebida);
        Assert.Equal("2026-11-20", previsao.Recebida!.DataPrevista);
        Assert.Equal(new[] { "2026-11-17", "2026-11-18", "2026-11-19" }, previsao.Recebida.Dias.Select(d => d.Dia));
    }

    [Fact]
    public async Task Alerta_SemServicoDePrevisao_DeveUsarAUltimaLeituraDosTotens()
    {
        var (service, _) = await CriarAsync(new PrevisaoFalsa(null));

        var alerta = await service.ObterAlertaHojeAsync(CancellationToken.None);

        Assert.Null(alerta.Probabilidade);
        Assert.Contains("totens", alerta.Fonte);
        Assert.False(alerta.Perigoso); // a leitura mais recente do seed é de nível "atencao"
    }

    [Fact]
    public async Task Pontos_DevemSeguirOContratoDoApp()
    {
        var (service, _) = await CriarAsync(new PrevisaoFalsa(null));

        var pontos = await service.ListarPontosAsync(CancellationToken.None);

        Assert.Equal(5, pontos.Count);
        Assert.All(pontos, p => Assert.False(string.IsNullOrWhiteSpace(p.Endereco)));
        Assert.Equal(34.2m, pontos[0].UltimaLeitura!.IndiceCalor);
        Assert.Equal("perigo", pontos[0].UltimaLeitura!.Nivel);
        Assert.Null(pontos[4].UltimaLeitura); // o totem TOT-005 ainda não enviou leitura
    }

    [Fact]
    public async Task Pausas_DevemSerListadasDaMaisRecenteParaAMaisAntiga()
    {
        var (service, _) = await CriarAsync(new PrevisaoFalsa(null));
        var cedo = new DateTimeOffset(2026, 11, 20, 13, 0, 0, TimeSpan.Zero);

        await service.RegistrarPausaAsync(new RegistrarPausaDto { TrabalhadorId = 1, RegistradaEm = cedo, DuracaoMin = 10, BebeuAgua = true }, CancellationToken.None);
        await service.RegistrarPausaAsync(new RegistrarPausaDto { TrabalhadorId = 1, RegistradaEm = cedo.AddHours(1), DuracaoMin = 5, BebeuAgua = false }, CancellationToken.None);
        await service.RegistrarPausaAsync(new RegistrarPausaDto { TrabalhadorId = 2, RegistradaEm = cedo, DuracaoMin = 15 }, CancellationToken.None);

        var lista = await service.ListarPausasAsync(1, CancellationToken.None);

        Assert.Equal(2, lista.Count);
        Assert.Equal(5, lista[0].DuracaoMin);
        Assert.False(lista[0].BebeuAgua);
    }

    [Fact]
    public async Task ClimaDiario_RegistrarOMesmoDiaDeveAtualizar()
    {
        var (service, db) = await CriarAsync(new PrevisaoFalsa(null));
        var dto = new RegistrarClimaDiarioDto
        {
            Dia = new DateTime(2026, 11, 19), TempMax = 29, TempMin = 18, TempMedia = 23, UmidMedia = 60, UmidMin = 40,
            IndiceCalorMax = 29.5, PressaoMedia = 925, RadiacaoTotal = 20000, PrecipTotal = 2, VentoMedio = 3
        };

        var resultado = await service.RegistrarClimaDiarioAsync(dto, CancellationToken.None);

        Assert.Equal("2026-11-19", resultado.Dia);
        Assert.Equal(29, resultado.TempMax);
        Assert.Equal(3, await db.ClimasDiarios.CountAsync());
    }
}
