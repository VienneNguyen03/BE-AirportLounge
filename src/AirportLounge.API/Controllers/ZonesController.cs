using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.LoungeZones.Commands;
using AirportLounge.Application.Features.LoungeZones.Queries;
using AirportLounge.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ZonesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ZonesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateZoneCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateZoneStatusCommand command)
    {
        if (id != command.ZoneId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginatedList<ZoneDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] ZoneStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetZonesQuery(search, status, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateZoneCommand command)
    {
        if (id != command.ZoneId) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteZoneCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("batch-delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBatch([FromBody] IReadOnlyList<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
            return BadRequest("No IDs provided");

        var result = await _mediator.Send(new DeleteZonesBatchCommand(ids));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<List<ZoneAlertDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAlerts()
    {
        var result = await _mediator.Send(new GetZoneAlertsQuery());
        return Ok(result);
    }
}
