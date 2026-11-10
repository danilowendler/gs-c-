using Microsoft.AspNetCore.Mvc;
using Refresca.Api.Application.DTOs;
using Refresca.Api.Application.Interfaces;

namespace Refresca.Api.Controllers;

[ApiController]
[Route("api/clima-diario")]
public sealed class ClimaDiarioController(IRefrescaService service) : ControllerBase
{
    /// <summary>Registra (ou atualiza) o resumo do clima de um dia, usado no alerta do dia seguinte.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClimaDiarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClimaDiarioDto>> Post([FromBody] RegistrarClimaDiarioDto dto, CancellationToken ct) =>
        Ok(await service.RegistrarClimaDiarioAsync(dto, ct));

    /// <summary>Últimos dias registrados, mais recentes primeiro.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClimaDiarioDto>>> Get([FromQuery] int dias = 7, CancellationToken ct = default) =>
        Ok(await service.ListarClimaDiarioAsync(dias, ct));
}
