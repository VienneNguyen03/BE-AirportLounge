using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Attendance.Commands;
using AirportLounge.Application.Features.Attendance.Queries;
using AirportLounge.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator) => _mediator = mediator;

    [HttpPost("check-in")]
    [ProducesResponseType(typeof(Result<CheckInResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("check-out")]
    [ProducesResponseType(typeof(Result<CheckOutResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("report")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<PaginatedList<AttendanceReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReport(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate,
        [FromQuery] Guid? employeeId, [FromQuery] AttendanceStatus? status,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(
            new GetAttendanceReportQuery(startDate, endDate, employeeId, status, search, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttendanceCommand command)
    {
        if (id != command.AttendanceId) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _mediator.Send(new ConfirmAttendanceCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Export(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate,
        [FromQuery] Guid? employeeId, [FromQuery] AttendanceStatus? status,
        [FromQuery] string format = "csv")
    {
        var result = await _mediator.Send(new ExportAttendanceQuery(startDate, endDate, employeeId, status));
        if (!result.IsSuccess || result.Data is null) return BadRequest(result);

        var filename = $"attendance_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
        return format.ToLowerInvariant() switch
        {
            "pdf" => File(
                AirportLounge.API.Services.AttendanceExportService.ToPdf(result.Data, startDate, endDate),
                "application/pdf",
                $"{filename}.pdf"),
            _ => File(
                System.Text.Encoding.UTF8.GetBytes(AirportLounge.API.Services.AttendanceExportService.ToCsv(result.Data)),
                "text/csv",
                $"{filename}.csv")
        };
    }
}
