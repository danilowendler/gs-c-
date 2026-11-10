using System.ComponentModel.DataAnnotations;

namespace Refresca.Api.Application.DTOs;

/// <summary>Contrato com o app: POST /api/pausas.</summary>
public sealed class RegistrarPausaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "trabalhadorId deve ser positivo.")]
    public int TrabalhadorId { get; init; }

    [Range(1, int.MaxValue)]
    public int? PontoRefrescaId { get; init; }

    [Required]
    public DateTimeOffset RegistradaEm { get; init; }

    [Range(1, 180)]
    public int DuracaoMin { get; init; } = 10;

    public bool BebeuAgua { get; init; } = true;
}
