using Microsoft.AspNetCore.Mvc;
using Refresca.Api.Application.DTOs;
using Refresca.Api.Application.Interfaces;

namespace Refresca.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class PausasController(IRefrescaService service) : ControllerBase
{
    /// <summary>Registra uma pausa de hidratação enviada pelo app.</summary>
    [HttpPost("pausas")]
    [ProducesResponseType(typeof(PausaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PausaResponseDto>> Post(
        [FromBody] RegistrarPausaDto dto, CancellationToken ct)
    {
        var result = await service.RegistrarPausaAsync(dto, ct);
        return Created($"/api/trabalhadores/{result.TrabalhadorId}/pausas", result);
    }

    /// <summary>Histórico de pausas de um trabalhador, mais recentes primeiro.</summary>
    [HttpGet("trabalhadores/{id:int}/pausas")]
    [ProducesResponseType(typeof(IReadOnlyList<PausaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PausaResponseDto>>> GetByWorker(int id, CancellationToken ct)
    {
        if (id <= 0) return BadRequest("Identificador inválido.");
        return Ok(await service.ListarPausasAsync(id, ct));
    }
}
