using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.IdCards.Commands;
using AirportLounge.Application.Features.IdCards.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IdCardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IdCardsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<List<IdCardDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        var result = await _mediator.Send(new GetEmployeeIdCardsQuery(employeeId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Issue([FromBody] IssueIdCardBody body)
    {
        var result = await _mediator.Send(new IssueIdCardCommand(body.EmployeeId, body.TemplateId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _mediator.Send(new ActivateIdCardCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/revoke")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke(Guid id, [FromBody] ReasonBody body)
    {
        var result = await _mediator.Send(new RevokeIdCardCommand(id, body.Reason));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/report")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReportIssue(Guid id, [FromBody] ReportIssueBody body)
    {
        var result = await _mediator.Send(new ReportIdCardIssueCommand(id, body.IsLost, body.Reason));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/request-reissue")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestReissue(Guid id, [FromBody] ReasonBody body)
    {
        var result = await _mediator.Send(new RequestIdCardReissueCommand(id, body.Reason));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/reissue")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reissue(Guid id, [FromBody] ReissueBody body)
    {
        var result = await _mediator.Send(new ReissueIdCardCommand(id, body.NewTemplateId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

public record IssueIdCardBody(Guid EmployeeId, Guid TemplateId);
public record ReasonBody(string Reason);
public record ReportIssueBody(bool IsLost, string Reason);
public record ReissueBody(Guid? NewTemplateId);
