using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Request More Info: UnderReview → NeedsInfo (Manager) ─────────
public record RequestMoreInfoCommand(
    Guid LeaveRequestId,
    string Comment) : IRequest<Result<bool>>;

public class RequestMoreInfoCommandHandler : IRequestHandler<RequestMoreInfoCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly INotificationService _notifications;

    public RequestMoreInfoCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICacheService cache, INotificationService notifications)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(RequestMoreInfoCommand request, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<bool>.Failure("Only managers can request more info");

        var lr = await _unitOfWork.LeaveRequests.Query()
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(x => x.Id == request.LeaveRequestId && !x.IsDeleted, ct);

        if (lr is null)
            return Result<bool>.Failure("Leave request not found");

        // Idempotent
        if (lr.Status == LeaveRequestStatus.NeedsInfo)
            return Result<bool>.Success(true, "Already in NeedsInfo status");

        if (lr.Status != LeaveRequestStatus.UnderReview)
            return Result<bool>.Failure($"Cannot request info: status is '{lr.Status}'");

        lr.Status = LeaveRequestStatus.NeedsInfo;
        lr.ReviewerComment = request.Comment;
        lr.UpdatedBy = _currentUser.Email;
        _unitOfWork.LeaveRequests.Update(lr);
        await _unitOfWork.SaveChangesAsync(ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, ct);

        // Notify the employee
        await _notifications.SendToUserAsync(
            lr.Employee.UserId,
            "More Information Needed",
            $"Your leave request ({lr.StartDate:yyyy-MM-dd} to {lr.EndDate:yyyy-MM-dd}) requires additional information: {request.Comment}",
            ct);

        return Result<bool>.Success(true, "More info requested from employee");
    }
}
