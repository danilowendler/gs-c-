using Microsoft.AspNetCore.Mvc;
using Refresca.Api.Application.DTOs;
using Refresca.Api.Application.Interfaces;

namespace Refresca.Api.Controllers;

[ApiController]
[Route("api/pontos")]
public sealed class PontosController(IRefrescaService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PontoResponseDto>>> Get(CancellationToken ct) =>
        Ok(await service.ListarPontosAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PontoResponseDto>> GetById(int id, CancellationToken ct)
    {
        var ponto = await service.ObterPontoAsync(id, ct);
        return ponto is null ? NotFound() : Ok(ponto);
    }
}
