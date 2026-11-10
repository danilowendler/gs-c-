using Microsoft.AspNetCore.Mvc;
using Refresca.Api.Application.DTOs;
using Refresca.Api.Application.Interfaces;

namespace Refresca.Api.Controllers;

[ApiController]
[Route("api/leituras")]
public sealed class LeiturasController(IRefrescaService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(LeituraResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<LeituraResponseDto>> Post(
        [FromBody] RegistrarLeituraDto dto, CancellationToken ct)
    {
        var result = await service.RegistrarLeituraAsync(dto, ct);
        return Created($"/api/leituras/{result.Id}", result);
    }
}
