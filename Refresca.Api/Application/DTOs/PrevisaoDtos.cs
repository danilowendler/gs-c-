namespace Refresca.Api.Application.DTOs;

/// <summary>Corpo do POST /previsoes no serviço de previsão (contrato da equipe).</summary>
public sealed record PrevisaoRequisicaoDto(string DataPrevista, IReadOnlyList<ClimaDiarioDto> Dias);

/// <summary>Resposta do POST /previsoes no serviço de previsão.</summary>
public sealed record PrevisaoResultadoDto(
    int Id, string DataPrevista, double Probabilidade, bool Perigoso, string VersaoModelo);
