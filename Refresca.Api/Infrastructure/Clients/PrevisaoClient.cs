using System.Net.Http.Json;
using System.Text.Json;
using Refresca.Api.Application.DTOs;
using Refresca.Api.Application.Interfaces;

namespace Refresca.Api.Infrastructure.Clients;

public sealed class PrevisaoClient(HttpClient http, ILogger<PrevisaoClient> logger) : IPrevisaoClient
{
    public async Task<PrevisaoResultadoDto?> PreverAsync(PrevisaoRequisicaoDto requisicao, CancellationToken ct)
    {
        try
        {
            using var resposta = await http.PostAsJsonAsync("previsoes", requisicao, ct);
            if (!resposta.IsSuccessStatusCode)
            {
                var corpo = await resposta.Content.ReadAsStringAsync(ct);
                logger.LogWarning("Serviço de previsão respondeu {Status}: {Corpo}", (int)resposta.StatusCode, corpo);
                return null;
            }
            return await resposta.Content.ReadFromJsonAsync<PrevisaoResultadoDto>(cancellationToken: ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Serviço de previsão indisponível em {Url}.", http.BaseAddress);
            return null;
        }
    }
}
