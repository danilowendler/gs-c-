using Microsoft.EntityFrameworkCore;
using Refresca.Api.Domain.Entities;

namespace Refresca.Api.Infrastructure.Persistence;

/// <summary>
/// Dados SIMULADOS para demonstração acadêmica. Os estabelecimentos são fictícios; os endereços
/// são ruas reais da Vila Mariana, com coordenadas aproximadas (os mesmos do modo demonstração do app).
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(RefrescaDbContext db, TimeProvider tempo)
    {
        var agora = tempo.GetUtcNow();

        if (!await db.PontosRefresca.AnyAsync())
        {
            db.PontosRefresca.AddRange(
                new PontoRefresca("Padaria parceira (exemplo)", "Rua Domingos de Morais, perto do metrô Vila Mariana",
                    "6h às 21h", -23.5893m, -46.6347m, true, true, true, "TOT-001"),
                new PontoRefresca("Banca parceira (exemplo)", "Rua Domingos de Morais, perto do metrô Santa Cruz",
                    "7h às 19h", -23.5990m, -46.6366m, true, true, false, "TOT-002"),
                new PontoRefresca("Farmácia parceira (exemplo)", "Rua Vergueiro, perto do metrô Ana Rosa",
                    "24 horas", -23.5813m, -46.6383m, true, false, true, "TOT-003"),
                new PontoRefresca("Lanchonete parceira (exemplo)", "Rua Sena Madureira",
                    "8h às 22h", -23.5918m, -46.6311m, true, true, true, "TOT-004"),
                new PontoRefresca("Mercadinho parceiro (exemplo)", "Rua Joaquim Távora",
                    "7h às 20h", -23.5952m, -46.6398m, true, false, false, "TOT-005"));
            await db.SaveChangesAsync();
        }

        if (!await db.Leituras.AnyAsync())
        {
            db.Leituras.AddRange(
                new Leitura("TOT-001", 32.0m, 50m, 34.2m, agora.AddMinutes(-4)),
                new Leitura("TOT-002", 30.0m, 52m, 31.0m, agora.AddMinutes(-7)),
                new Leitura("TOT-003", 28.8m, 55m, 29.4m, agora.AddMinutes(-2)),
                new Leitura("TOT-004", 31.2m, 51m, 33.1m, agora.AddMinutes(-12)));
            await db.SaveChangesAsync();
        }

        // Garante o resumo do clima dos 3 dias anteriores a hoje, usado no alerta do dia.
        // Valores simulados de uma sequência de dias quentes; em produção viriam da estação do INMET
        // pelo POST /api/clima-diario.
        var hoje = RefrescaRelogio.HojeEmSaoPaulo(tempo);
        var simulados = new (int DiasAtras, double TempMax, double IndiceCalorMax)[] { (3, 32.0, 33.0), (2, 33.0, 34.0), (1, 34.0, 35.2) };
        foreach (var (diasAtras, tempMax, indiceCalorMax) in simulados)
        {
            var dia = hoje.AddDays(-diasAtras);
            if (await db.ClimasDiarios.AnyAsync(c => c.Dia == dia)) continue;
            db.ClimasDiarios.Add(new ClimaDiario(dia, tempMax: tempMax, tempMin: 20.5, tempMedia: 26.5, umidMedia: 50,
                umidMin: 30, indiceCalorMax: indiceCalorMax, pressaoMedia: 923, radiacaoTotal: 24000, precipTotal: 0, ventoMedio: 2));
        }
        await db.SaveChangesAsync();
    }
}
