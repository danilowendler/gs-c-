namespace Refresca.Api.Application.DTOs;

public sealed record LeituraResponseDto(
    int Id, string TotemId, decimal Temperatura, decimal Umidade,
    decimal IndiceCalor, string Nivel, DateTimeOffset MedidoEm);
