namespace Refresca.Api.Domain.Entities;

/// <summary>
/// Resumo do clima de um dia na cidade (as mesmas variáveis usadas no treino do modelo de ML).
/// Os 3 últimos dias são enviados ao serviço de previsão para gerar o alerta do dia.
/// </summary>
public sealed class ClimaDiario
{
    public int Id { get; private set; }
    public DateTime Dia { get; private set; }
    public double TempMax { get; private set; }
    public double TempMin { get; private set; }
    public double TempMedia { get; private set; }
    public double UmidMedia { get; private set; }
    public double UmidMin { get; private set; }
    public double IndiceCalorMax { get; private set; }
    public double PressaoMedia { get; private set; }
    public double RadiacaoTotal { get; private set; }
    public double PrecipTotal { get; private set; }
    public double VentoMedio { get; private set; }

    private ClimaDiario() { }

    public ClimaDiario(DateTime dia, double tempMax, double tempMin, double tempMedia, double umidMedia,
        double umidMin, double indiceCalorMax, double pressaoMedia, double radiacaoTotal,
        double precipTotal, double ventoMedio)
    {
        Dia = dia.Date;
        Atualizar(tempMax, tempMin, tempMedia, umidMedia, umidMin, indiceCalorMax,
            pressaoMedia, radiacaoTotal, precipTotal, ventoMedio);
    }

    public void Atualizar(double tempMax, double tempMin, double tempMedia, double umidMedia,
        double umidMin, double indiceCalorMax, double pressaoMedia, double radiacaoTotal,
        double precipTotal, double ventoMedio)
    {
        if (tempMin > tempMax)
            throw new ArgumentException("A temperatura mínima não pode ser maior que a máxima.");
        if (tempMax is < -10 or > 50 || tempMin is < -10 or > 50 || tempMedia is < -10 or > 50)
            throw new ArgumentOutOfRangeException(nameof(tempMax), "Temperaturas devem ficar entre -10 e 50 °C.");
        if (umidMedia is < 0 or > 100 || umidMin is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(umidMedia), "Umidade deve estar entre 0 e 100%.");
        if (pressaoMedia is < 800 or > 1100)
            throw new ArgumentOutOfRangeException(nameof(pressaoMedia), "Pressão deve estar entre 800 e 1100 mB.");
        if (radiacaoTotal < 0 || precipTotal < 0 || ventoMedio < 0)
            throw new ArgumentOutOfRangeException(nameof(radiacaoTotal), "Radiação, chuva e vento não podem ser negativos.");

        TempMax = tempMax;
        TempMin = tempMin;
        TempMedia = tempMedia;
        UmidMedia = umidMedia;
        UmidMin = umidMin;
        IndiceCalorMax = indiceCalorMax;
        PressaoMedia = pressaoMedia;
        RadiacaoTotal = radiacaoTotal;
        PrecipTotal = precipTotal;
        VentoMedio = ventoMedio;
    }
}
