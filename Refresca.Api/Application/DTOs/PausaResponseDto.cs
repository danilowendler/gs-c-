namespace Refresca.Api.Application.DTOs;

public sealed record PausaResponseDto(
    int Id, int TrabalhadorId, int? PontoRefrescaId,
    DateTimeOffset RegistradaEm, int DuracaoMin, bool BebeuAgua);
