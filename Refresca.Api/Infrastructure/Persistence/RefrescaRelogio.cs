namespace Refresca.Api.Infrastructure.Persistence;

public static class RefrescaRelogio
{
    /// <summary>Data de hoje em São Paulo (UTC-3; o Brasil não tem horário de verão desde 2019).</summary>
    public static DateTime HojeEmSaoPaulo(TimeProvider tempo) =>
        tempo.GetUtcNow().UtcDateTime.AddHours(-3).Date;
}
