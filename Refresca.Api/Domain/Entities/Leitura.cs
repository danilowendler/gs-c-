namespace Refresca.Api.Domain.Entities;

public sealed class Leitura
{
    public int Id { get; private set; }
    public string TotemId { get; private set; } = string.Empty;
    public decimal Temperatura { get; private set; }
    public decimal Umidade { get; private set; }
    public decimal IndiceCalor { get; private set; }
    public string Nivel { get; private set; } = "normal";
    public DateTimeOffset MedidoEm { get; private set; }

    private Leitura() { }

    public Leitura(string totemId, decimal temperatura, decimal umidade,
        decimal indiceCalor, DateTimeOffset medidoEm)
    {
        if (string.IsNullOrWhiteSpace(totemId))
            throw new ArgumentException("O identificador do totem é obrigatório.", nameof(totemId));
        if (temperatura is < -20 or > 60)
            throw new ArgumentOutOfRangeException(nameof(temperatura), "Temperatura fora da faixa aceita.");
        if (umidade is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(umidade), "Umidade deve estar entre 0 e 100%.");
        if (indiceCalor is < -20 or > 80)
            throw new ArgumentOutOfRangeException(nameof(indiceCalor), "Índice de calor fora da faixa aceita.");

        TotemId = totemId.Trim();
        Temperatura = temperatura;
        Umidade = umidade;
        IndiceCalor = indiceCalor;
        // Guardado sempre em UTC: com o mesmo fuso em todas as linhas, a ordenação no banco fica correta.
        MedidoEm = medidoEm.ToUniversalTime();
        Nivel = ClassificarNivel(indiceCalor);
    }

    public static string ClassificarNivel(decimal indiceCalor) =>
        indiceCalor >= 32m ? "perigo" :
        indiceCalor >= 27m ? "atencao" : "normal";
}
