using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Onboarding.Commands;
using AirportLounge.Application.Features.Onboarding.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOnboardingCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{processId:guid}/start")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start(Guid processId)
    {
        var result = await _mediator.Send(new StartOnboardingCommand(processId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{processId:guid}/block")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Block(Guid processId, [FromBody] BlockOnboardingRequest? body)
    {
        var result = await _mediator.Send(new BlockOnboardingCommand(processId, body?.Reason));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{processId:guid}/unblock")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unblock(Guid processId)
    {
        var result = await _mediator.Send(new UnblockOnboardingCommand(processId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{processId:guid}/activate")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(Guid processId)
    {
        var result = await _mediator.Send(new ActivateOnboardingCommand(processId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("tasks/{id:guid}/complete")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTask(Guid id)
    {
        var result = await _mediator.Send(new CompleteOnboardingTaskCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<OnboardingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid employeeId)
    {
        var result = await _mediator.Send(new GetOnboardingQuery(employeeId));
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}

public record BlockOnboardingRequest(string? Reason);
