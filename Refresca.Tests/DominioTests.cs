using Xunit;
using Refresca.Api.Domain.Entities;

namespace Refresca.Tests;

public class DominioTests
{
    [Fact]
    public void Pausa_DeveRejeitarTrabalhadorInvalido()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PausaHidratacao(0, null, DateTimeOffset.UtcNow, 10, true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(181)]
    public void Pausa_DeveRejeitarDuracaoForaDaFaixa(int duracao)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PausaHidratacao(1, null, DateTimeOffset.UtcNow, duracao, true));
    }

    [Fact]
    public void ClimaDiario_DeveRejeitarMinimaMaiorQueMaxima()
    {
        Assert.Throws<ArgumentException>(() =>
            new ClimaDiario(new DateTime(2026, 11, 19), tempMax: 25, tempMin: 30, tempMedia: 27, umidMedia: 50,
                umidMin: 30, indiceCalorMax: 26, pressaoMedia: 923, radiacaoTotal: 20000, precipTotal: 0, ventoMedio: 2));
    }

    [Fact]
    public void ClimaDiario_DeveGuardarSoAData()
    {
        var clima = new ClimaDiario(new DateTime(2026, 11, 19, 15, 30, 0), tempMax: 33, tempMin: 20, tempMedia: 26,
            umidMedia: 50, umidMin: 30, indiceCalorMax: 34, pressaoMedia: 923, radiacaoTotal: 24000, precipTotal: 0, ventoMedio: 2);
        Assert.Equal(new DateTime(2026, 11, 19), clima.Dia);
    }
}
