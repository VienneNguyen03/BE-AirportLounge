using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Open for Review: Submitted → UnderReview (Manager only) ──────
public record OpenLeaveForReviewCommand(Guid LeaveRequestId) : IRequest<Result<bool>>;

public class OpenLeaveForReviewCommandHandler : IRequestHandler<OpenLeaveForReviewCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public OpenLeaveForReviewCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(OpenLeaveForReviewCommand request, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<bool>.Failure("Only managers can open leave requests for review");

        var lr = await _unitOfWork.LeaveRequests.GetByIdAsync(request.LeaveRequestId, ct);
        if (lr is null || lr.IsDeleted)
            return Result<bool>.Failure("Leave request not found");

        // Idempotent
        if (lr.Status == LeaveRequestStatus.UnderReview)
            return Result<bool>.Success(true, "Already under review");

        if (lr.Status != LeaveRequestStatus.Submitted)
            return Result<bool>.Failure($"Cannot open for review: status is '{lr.Status}'");

        lr.Status = LeaveRequestStatus.UnderReview;
        lr.ManagerId = _currentUser.UserId;
        lr.UpdatedBy = _currentUser.Email;
        _unitOfWork.LeaveRequests.Update(lr);
        await _unitOfWork.SaveChangesAsync(ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, ct);

        return Result<bool>.Success(true, "Leave request is now under review");
    }
}
