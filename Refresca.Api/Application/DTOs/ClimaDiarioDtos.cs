using System.ComponentModel.DataAnnotations;

namespace Refresca.Api.Application.DTOs;

/// <summary>Resumo do clima de um dia (POST /api/clima-diario). Registrar de novo o mesmo dia atualiza os valores.</summary>
public sealed class RegistrarClimaDiarioDto
{
    [Required]
    public DateTime Dia { get; init; }

    [Range(-10, 50)] public double TempMax { get; init; }
    [Range(-10, 50)] public double TempMin { get; init; }
    [Range(-10, 50)] public double TempMedia { get; init; }
    [Range(0, 100)] public double UmidMedia { get; init; }
    [Range(0, 100)] public double UmidMin { get; init; }
    [Range(-10, 70)] public double IndiceCalorMax { get; init; }
    [Range(800, 1100)] public double PressaoMedia { get; init; }
    [Range(0, 60000)] public double RadiacaoTotal { get; init; }
    [Range(0, 500)] public double PrecipTotal { get; init; }
    [Range(0, 60)] public double VentoMedio { get; init; }
}

public sealed record ClimaDiarioDto(
    string Dia, double TempMax, double TempMin, double TempMedia, double UmidMedia, double UmidMin,
    double IndiceCalorMax, double PressaoMedia, double RadiacaoTotal, double PrecipTotal, double VentoMedio);
