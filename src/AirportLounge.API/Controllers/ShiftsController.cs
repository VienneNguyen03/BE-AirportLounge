using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Shifts.Commands;
using AirportLounge.Application.Features.Shifts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShiftsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginatedList<ShiftDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetShiftsQuery(search, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateShiftCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("assign/batch")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<List<Guid>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignBatch([FromBody] AssignShiftsBatchCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Assign([FromBody] AssignShiftCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("schedule")]
    [ProducesResponseType(typeof(Result<PaginatedList<ScheduleItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSchedule(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid? employeeId,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetScheduleQuery(startDate, endDate, employeeId, search, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShiftCommand command)
    {
        if (id != command.ShiftId) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteShiftCommand(id));
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

        var result = await _mediator.Send(new DeleteShiftsBatchCommand(ids));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("assignments/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unassign(Guid id)
    {
        var result = await _mediator.Send(new UnassignShiftCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("assignments/batch-delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnassignBatch([FromBody] IReadOnlyList<Guid> assignmentIds)
    {
        if (assignmentIds is null || assignmentIds.Count == 0)
            return BadRequest("No IDs provided");

        var result = await _mediator.Send(new UnassignShiftsBatchCommand(assignmentIds));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("assignments/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAssignment(Guid id, [FromBody] UpdateShiftAssignmentCommand command)
    {
        if (id != command.AssignmentId) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
