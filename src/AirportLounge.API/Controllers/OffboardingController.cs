using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Offboarding.Commands;
using AirportLounge.Application.Features.Offboarding.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OffboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OffboardingController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Initiate([FromBody] InitiateOffboardingCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{processId:guid}/{action}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transition(Guid processId, string action)
    {
        var validActions = new[] { "start", "asset-recovery", "access-revocation", "final-settlement", "complete" };
        if (!validActions.Contains(action))
            return BadRequest(Result<bool>.Failure($"Invalid action: {action}"));

        var result = await _mediator.Send(new TransitionOffboardingCommand(processId, action));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("tasks/{taskId:guid}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTask(Guid taskId)
    {
        var result = await _mediator.Send(new CompleteOffboardingTaskCommand(taskId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOffboardingCommand command)
    {
        if (id != command.ProcessId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<OffboardingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid employeeId)
    {
        var result = await _mediator.Send(new GetOffboardingQuery(employeeId));
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}
