using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.MasterData.Commands;
using AirportLounge.Application.Features.MasterData.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/masterdata")]
[Authorize(Roles = "Admin,Manager")]
public class MasterDataController : ControllerBase
{
    private readonly IMediator _mediator;

    public MasterDataController(IMediator mediator) => _mediator = mediator;

    // --- DEPARTMENTS ---
    
    [HttpGet("departments")]
    [ProducesResponseType(typeof(Result<PaginatedList<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetDepartmentsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("departments")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("departments/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("departments/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var result = await _mediator.Send(new DeleteDepartmentCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // --- POSITIONS ---

    [HttpGet("positions")]
    [ProducesResponseType(typeof(Result<PaginatedList<PositionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetPositionsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("positions")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePosition([FromBody] CreatePositionCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("positions/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] UpdatePositionCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("positions/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePosition(Guid id)
    {
        var result = await _mediator.Send(new DeletePositionCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // --- SKILLS ---

    [HttpGet("skills")]
    [ProducesResponseType(typeof(Result<PaginatedList<SkillDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetSkillsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("skills")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("skills/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] UpdateSkillCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("skills/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteSkill(Guid id)
    {
        var result = await _mediator.Send(new DeleteSkillCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // --- TASK CATEGORIES ---

    [HttpGet("task-categories")]
    [ProducesResponseType(typeof(Result<PaginatedList<TaskCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskCategories([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetTaskCategoriesQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("task-categories")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTaskCategory([FromBody] CreateTaskCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("task-categories/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTaskCategory(Guid id, [FromBody] UpdateTaskCategoryCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("task-categories/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTaskCategory(Guid id)
    {
        var result = await _mediator.Send(new DeleteTaskCategoryCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
