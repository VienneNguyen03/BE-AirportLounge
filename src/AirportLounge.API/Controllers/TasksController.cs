using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Tasks.Commands;
using AirportLounge.Application.Features.Tasks.Queries;
using AirportLounge.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusCommand command)
    {
        if (id != command.TaskId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginatedList<TaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] TaskItemStatus? status, [FromQuery] Guid? assignedToId,
        [FromQuery] TaskPriority? priority,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(
            new GetTasksQuery(status, assignedToId, priority, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Export(
        [FromQuery] TaskItemStatus? status, [FromQuery] Guid? assignedToId,
        [FromQuery] TaskPriority? priority, [FromQuery] string format = "csv")
    {
        var result = await _mediator.Send(new ExportTasksQuery(status, assignedToId, priority));
        if (!result.IsSuccess || result.Data is null) return BadRequest(result);

        var filename = $"tasks_{DateTime.UtcNow:yyyyMMdd}";
        return format.ToLowerInvariant() switch
        {
            "pdf" => File(
                AirportLounge.API.Services.TaskExportService.ToPdf(result.Data),
                "application/pdf",
                $"{filename}.pdf"),
            _ => File(
                System.Text.Encoding.UTF8.GetBytes(AirportLounge.API.Services.TaskExportService.ToCsv(result.Data)),
                "text/csv",
                $"{filename}.csv")
        };
    }
}
