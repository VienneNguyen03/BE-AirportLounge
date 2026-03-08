using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Training.Commands;
using AirportLounge.Application.Features.Training.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrainingController(IMediator mediator) => _mediator = mediator;

    [HttpGet("courses")]
    [ProducesResponseType(typeof(Result<List<TrainingCourseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourses()
    {
        var result = await _mediator.Send(new GetTrainingCoursesQuery());
        return Ok(result);
    }

    [HttpPost("courses")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateTrainingCourseCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("enroll")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Enroll([FromBody] EnrollInCourseCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("enrollments/{id:guid}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteTrainingCommand command)
    {
        if (id != command.EnrollmentId)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("enrollments/{employeeId:guid}")]
    [ProducesResponseType(typeof(Result<List<TrainingEnrollmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEnrollments(Guid employeeId)
    {
        var result = await _mediator.Send(new GetTrainingEnrollmentsQuery(employeeId));
        return Ok(result);
    }
}
