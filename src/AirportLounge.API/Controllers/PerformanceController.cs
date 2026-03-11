using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Performance.Commands;
using AirportLounge.Application.Features.Performance.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerformanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public PerformanceController(IMediator mediator) => _mediator = mediator;

    // ── Goals ──────────────────────────────────────

    [HttpPost("goals")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGoal([FromBody] CreatePerformanceGoalCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("goals/{id:guid}/progress")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] UpdateGoalProgressCommand command)
    {
        if (id != command.GoalId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("goals/{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<List<PerformanceGoalDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoals(Guid employeeId)
    {
        var result = await _mediator.Send(new GetPerformanceGoalsQuery(employeeId));
        return Ok(result);
    }

    // ── Reviews ────────────────────────────────────

    [HttpPost("reviews")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReview([FromBody] CreatePerformanceReviewCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("reviews/{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<List<PerformanceReviewDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviews(Guid employeeId)
    {
        var result = await _mediator.Send(new GetPerformanceReviewsQuery(employeeId));
        return Ok(result);
    }

    // ── Review Workflow Transitions ────────────────

    [HttpPost("reviews/{reviewId:guid}/{action}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransitionReview(Guid reviewId, string action)
    {
        var result = await _mediator.Send(new TransitionReviewCommand(reviewId, action));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // ── Self Assessment ────────────────────────────

    [HttpPost("reviews/{id:guid}/self-assessment")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitSelfAssessment(Guid id, [FromBody] SubmitSelfAssessmentCommand command)
    {
        if (id != command.ReviewId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // ── Manager Complete ───────────────────────────

    [HttpPost("reviews/{id:guid}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteReview(Guid id, [FromBody] CompleteReviewCommand command)
    {
        if (id != command.ReviewId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // ── Peer Feedback ──────────────────────────────

    [HttpPost("reviews/{reviewId:guid}/feedback")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPeerFeedback(Guid reviewId, [FromBody] AddPeerFeedbackCommand command)
    {
        if (reviewId != command.ReviewId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
