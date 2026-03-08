using AirportLounge.Application.Common.Models;
using AirportLounge.Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirportLounge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("staff/{employeeId:guid}")]
    [Authorize(Roles = "Staff,Manager,Admin")]
    [ProducesResponseType(typeof(Result<StaffDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStaffDashboard(Guid employeeId)
    {
        var result = await _mediator.Send(new GetStaffDashboardQuery(employeeId));
        return Ok(result);
    }

    [HttpGet("manager")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(Result<ManagerDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetManagerDashboard()
    {
        var result = await _mediator.Send(new GetManagerDashboardQuery());
        return Ok(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<AdminDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var result = await _mediator.Send(new GetAdminDashboardQuery());
        return Ok(result);
    }
}
