using Xunit;
using Refresca.Api.Domain.Entities;

namespace Refresca.Tests;

public class LeituraTests
{
    [Theory]
    [InlineData(26.9, "normal")]
    [InlineData(27.0, "atencao")]
    [InlineData(31.9, "atencao")]
    [InlineData(32.0, "perigo")]
    [InlineData(35.1, "perigo")]
    public void ClassificarNivel_DeveRespeitarFaixasDoContrato(decimal indice, string esperado)
    {
        Assert.Equal(esperado, Leitura.ClassificarNivel(indice));
    }

    [Fact]
    public void Construtor_DeveRejeitarUmidadeForaDaFaixa()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Leitura("TOT-001", 30m, 101m, 32m, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Construtor_DeveRejeitarTotemVazio()
    {
        Assert.Throws<ArgumentException>(() =>
            new Leitura(" ", 30m, 50m, 25m, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Construtor_DeveGuardarMomentoEmUtc()
    {
        var emSaoPaulo = new DateTimeOffset(2026, 11, 19, 14, 0, 0, TimeSpan.FromHours(-3));
        var leitura = new Leitura("TOT-001", 33m, 48m, 35.1m, emSaoPaulo);
        Assert.Equal(TimeSpan.Zero, leitura.MedidoEm.Offset);
        Assert.Equal(emSaoPaulo, leitura.MedidoEm); // mesmo instante
    }
}
