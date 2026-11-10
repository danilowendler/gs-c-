namespace Refresca.Api.Domain.Entities;

public sealed class PausaHidratacao
{
    public int Id { get; private set; }
    public int TrabalhadorId { get; private set; }
    public int? PontoRefrescaId { get; private set; }
    public DateTimeOffset RegistradaEm { get; private set; }
    public int DuracaoMin { get; private set; }
    public bool BebeuAgua { get; private set; }
    public PontoRefresca? PontoRefresca { get; private set; }

    private PausaHidratacao() { }

    public PausaHidratacao(int trabalhadorId, int? pontoRefrescaId,
        DateTimeOffset registradaEm, int duracaoMin, bool bebeuAgua)
    {
        if (trabalhadorId <= 0)
            throw new ArgumentOutOfRangeException(nameof(trabalhadorId), "O identificador do trabalhador deve ser positivo.");
        if (duracaoMin is < 1 or > 180)
            throw new ArgumentOutOfRangeException(nameof(duracaoMin), "A duração deve estar entre 1 e 180 minutos.");

        TrabalhadorId = trabalhadorId;
        PontoRefrescaId = pontoRefrescaId;
        // Guardado sempre em UTC, para a ordenação no banco ficar correta.
        RegistradaEm = registradaEm.ToUniversalTime();
        DuracaoMin = duracaoMin;
        BebeuAgua = bebeuAgua;
    }
}
