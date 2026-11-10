namespace Refresca.Api.Application.DTOs;

public sealed record UltimaLeituraDto(decimal IndiceCalor, string Nivel, DateTimeOffset MedidoEm);

/// <summary>Contrato com o app: GET /api/pontos e GET /api/pontos/{id}.</summary>
public sealed record PontoResponseDto(
    int Id, string Nome, string Endereco, decimal Latitude, decimal Longitude,
    bool TemAgua, bool TemSombra, bool TemBanheiro, string Horario, UltimaLeituraDto? UltimaLeitura);
