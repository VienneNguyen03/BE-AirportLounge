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

    // ── Leave Types ──────────────────────────────────────────────

    [HttpGet("types")]
    [ProducesResponseType(typeof(Result<PaginatedList<LeaveTypeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveTypes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetLeaveTypesQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("types")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLeaveType([FromBody] CreateLeaveTypeCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("types/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLeaveType(Guid id, [FromBody] UpdateLeaveTypeCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("types/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteLeaveType(Guid id)
    {
        var result = await _mediator.Send(new DeleteLeaveTypeCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // ── Leave Balance ────────────────────────────────────────────

    [HttpPost("balance")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigureBalance([FromBody] ConfigureLeaveBalanceCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("balance/{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<List<LeaveBalanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance(Guid employeeId, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetLeaveBalanceQuery(employeeId, year ?? DateTime.UtcNow.Year));
        return Ok(result);
    }

    // ── Leave Requests (list / queries) ──────────────────────────

    [HttpGet("requests")]
    [ProducesResponseType(typeof(Result<PaginatedList<LeaveRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequests(
        [FromQuery] Guid? employeeId, [FromQuery] LeaveRequestStatus? status,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetLeaveRequestsQuery(
            employeeId, status, startDate, endDate, search, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("requests/pending")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<List<PendingLeaveRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending()
    {
        var result = await _mediator.Send(new GetPendingLeaveRequestsQuery());
        return Ok(result);
    }

    // ── Workflow Transitions ─────────────────────────────────────

    /// <summary>Create a new leave request in Draft status.</summary>
    [HttpPost("requests")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDraft([FromBody] CreateLeaveRequestDraftCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Submit a Draft or NeedsInfo request → Submitted.</summary>
    [HttpPost("requests/{id:guid}/submit")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _mediator.Send(new SubmitLeaveRequestCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Manager opens a Submitted request for review → UnderReview.</summary>
    [HttpPost("requests/{id:guid}/open")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OpenForReview(Guid id)
    {
        var result = await _mediator.Send(new OpenLeaveForReviewCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Manager requests more info → NeedsInfo.</summary>
    [HttpPost("requests/{id:guid}/request-more-info")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestMoreInfo(Guid id, [FromBody] RequestMoreInfoBody body)
    {
        var result = await _mediator.Send(new RequestMoreInfoCommand(id, body.Comment));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Manager approves → Approved.</summary>
    [HttpPost("requests/{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewBody body)
    {
        var result = await _mediator.Send(new ApproveLeaveRequestCommand(id, body.Comment));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Manager rejects → Rejected.</summary>
    [HttpPost("requests/{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewBody body)
    {
        var result = await _mediator.Send(new RejectLeaveRequestCommand(id, body.Comment));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cancel a request (Draft, Submitted, Approved, Scheduled).</summary>
    [HttpPost("requests/{id:guid}/cancel")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelLeaveRequestCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // ── Admin: Batch Delete ──────────────────────────────────────

    [HttpPost("requests/batch-delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRequestsBatch([FromBody] IReadOnlyList<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
            return BadRequest("No IDs provided");
        var result = await _mediator.Send(new DeleteLeaveRequestsBatchCommand(ids));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

// ── Request body models ──────────────────────────────────────────
public record ReviewBody(string? Comment);
public record RequestMoreInfoBody(string Comment);
