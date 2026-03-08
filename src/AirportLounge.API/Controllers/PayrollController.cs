using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Payroll.Commands;
using AirportLounge.Application.Features.Payroll.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayrollController(IMediator mediator) => _mediator = mediator;

    [HttpPost("salary")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetSalary([FromBody] SetSalaryStructureCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("salary/{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<SalaryStructureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSalary(Guid employeeId)
    {
        var result = await _mediator.Send(new GetSalaryStructureQuery(employeeId));
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreatePayrollCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApprovePayrollCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("records/{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<List<PayrollRecordDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecords(Guid employeeId, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetPayrollRecordsQuery(employeeId, year ?? DateTime.UtcNow.Year));
        return Ok(result);
    }
}
