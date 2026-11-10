using Microsoft.AspNetCore.Mvc;
using Refresca.Api.Application.DTOs;
using Refresca.Api.Application.Interfaces;

namespace Refresca.Api.Controllers;

[ApiController]
[Route("api/alertas")]
public sealed class AlertasController(IRefrescaService service) : ControllerBase
{
    /// <summary>Alerta de calor do dia: vem do serviço de previsão (modelo de ML) ou, se ele estiver fora do ar, dos totens.</summary>
    [HttpGet("hoje")]
    [ProducesResponseType(typeof(AlertaHojeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlertaHojeDto>> GetHoje(CancellationToken ct) =>
        Ok(await service.ObterAlertaHojeAsync(ct));
}
