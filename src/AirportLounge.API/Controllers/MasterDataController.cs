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
    [ProducesResponseType(typeof(Result<PaginatedList<MasterDataDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetDepartmentsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("departments")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateDepartment([FromBody] MasterDataBody body)
    {
        var result = await _mediator.Send(new CreateMasterDataCommand(MasterDataType.Department, body.Name, body.Description));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("departments/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] MasterDataBody body)
    {
        var result = await _mediator.Send(new UpdateMasterDataCommand(MasterDataType.Department, id, body.Name, body.Description, body.IsActive));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("departments/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var result = await _mediator.Send(new DeleteMasterDataCommand(MasterDataType.Department, id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // --- POSITIONS ---

    [HttpGet("positions")]
    [ProducesResponseType(typeof(Result<PaginatedList<MasterDataDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetPositionsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("positions")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePosition([FromBody] MasterDataBody body)
    {
        var result = await _mediator.Send(new CreateMasterDataCommand(MasterDataType.Position, body.Name, body.Description));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("positions/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] MasterDataBody body)
    {
        var result = await _mediator.Send(new UpdateMasterDataCommand(MasterDataType.Position, id, body.Name, body.Description, body.IsActive));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("positions/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePosition(Guid id)
    {
        var result = await _mediator.Send(new DeleteMasterDataCommand(MasterDataType.Position, id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // --- SKILLS ---

    [HttpGet("skills")]
    [ProducesResponseType(typeof(Result<PaginatedList<MasterDataDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetSkillsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("skills")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSkill([FromBody] MasterDataBody body)
    {
        var result = await _mediator.Send(new CreateMasterDataCommand(MasterDataType.Skill, body.Name, body.Description));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("skills/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] MasterDataBody body)
    {
        var result = await _mediator.Send(new UpdateMasterDataCommand(MasterDataType.Skill, id, body.Name, body.Description, body.IsActive));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("skills/{id:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteSkill(Guid id)
    {
        var result = await _mediator.Send(new DeleteMasterDataCommand(MasterDataType.Skill, id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

public record MasterDataBody(string Name, string? Description, bool IsActive = true);
