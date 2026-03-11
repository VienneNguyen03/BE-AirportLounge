using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Create Draft ─────────────────────────────────────────────────
public record CreateLeaveRequestDraftCommand(
    Guid LeaveTypeId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsHalfDay,
    string? Reason) : IRequest<Result<Guid>>;

public class CreateLeaveRequestDraftCommandHandler : IRequestHandler<CreateLeaveRequestDraftCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateLeaveRequestDraftCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateLeaveRequestDraftCommand request, CancellationToken ct)
    {
        if (_currentUser.UserId is null)
            return Result<Guid>.Failure("User not authenticated");

        var employee = await _unitOfWork.Employees.Query()
            .FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId.Value && !e.IsDeleted, ct);
        if (employee is null)
            return Result<Guid>.Failure("Employee profile not found");

        if (request.StartDate.Date > request.EndDate.Date)
            return Result<Guid>.Failure("Start date must be on or before end date");

        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(request.LeaveTypeId, ct);
        if (leaveType is null || !leaveType.IsActive)
            return Result<Guid>.Failure("Leave type not found or inactive");

        var totalDays = request.IsHalfDay
            ? 0.5m
            : (decimal)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1;

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            TotalDays = totalDays,
            IsHalfDay = request.IsHalfDay,
            Reason = request.Reason,
            Status = LeaveRequestStatus.Draft,
            CreatedBy = _currentUser.Email
        };

        await _unitOfWork.LeaveRequests.AddAsync(leaveRequest, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, ct);

        return Result<Guid>.Success(leaveRequest.Id, "Leave request draft created");
    }
}
