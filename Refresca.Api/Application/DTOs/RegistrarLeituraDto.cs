
using System.ComponentModel.DataAnnotations;

namespace Refresca.Api.Application.DTOs;

public sealed class RegistrarLeituraDto
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string TotemId { get; init; } = string.Empty;

    [Range(-20, 60)]
    public decimal Temperatura { get; init; }

    [Range(0, 100)]
    public decimal Umidade { get; init; }

    [Range(-20, 80)]
    public decimal IndiceCalor { get; init; }

    [Required]
    [RegularExpression("^(normal|atencao|perigo)$",
        ErrorMessage = "O nível deve ser normal, atencao ou perigo.")]
    public string Nivel { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset MedidoEm { get; init; }
}
