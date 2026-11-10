using Refresca.Api.Application.DTOs;

namespace Refresca.Api.Application.Interfaces;

/// <summary>Cliente do serviço de previsão (repositório de SOA, POST /previsoes).</summary>
public interface IPrevisaoClient
{
    /// <summary>Devolve null quando o serviço está fora do ar ou responde com erro.</summary>
    Task<PrevisaoResultadoDto?> PreverAsync(PrevisaoRequisicaoDto requisicao, CancellationToken ct);
}
