using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Leaves.Commands;
using AirportLounge.Application.Features.Leaves.Queries;
using AirportLounge.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeavesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeavesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("types")]
    [ProducesResponseType(typeof(Result<List<LeaveTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var result = await _mediator.Send(new GetLeaveTypesQuery());
        return Ok(result);
    }

    [HttpPost("types")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateLeaveType([FromBody] CreateLeaveTypeCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("balance")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfigureBalance([FromBody] ConfigureLeaveBalanceCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("balance/{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<List<LeaveBalanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBalance(Guid employeeId, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetLeaveBalanceQuery(employeeId, year ?? DateTime.UtcNow.Year));
        return Ok(result);
    }

    [HttpPost("requests")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitRequest([FromBody] SubmitLeaveRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("requests/{id:guid}/review")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReviewRequest(Guid id, [FromBody] ReviewLeaveRequestCommand command)
    {
        if (id != command.LeaveRequestId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("requests")]
    [ProducesResponseType(typeof(Result<PaginatedList<LeaveRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRequests(
        [FromQuery] Guid? employeeId, [FromQuery] LeaveRequestStatus? status,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetLeaveRequestsQuery(employeeId, status, startDate, endDate, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("requests/pending")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<List<PendingLeaveRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPending()
    {
        var result = await _mediator.Send(new GetPendingLeaveRequestsQuery());
        return Ok(result);
    }
}
