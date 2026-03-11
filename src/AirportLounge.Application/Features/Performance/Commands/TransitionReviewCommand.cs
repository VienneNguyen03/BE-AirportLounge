using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Performance.Commands;

/// <summary>
/// Handles all generic workflow transitions for performance reviews.
/// NotStarted→SelfInProgress, SelfSubmitted→PeerReviewOpen, PeerReviewDone→ManagerReview,
/// ManagerSubmitted→Calibration, Calibration→Finalized
/// </summary>
public record TransitionReviewCommand(Guid ReviewId, string Action) : IRequest<Result<bool>>;

public class TransitionReviewCommandHandler : IRequestHandler<TransitionReviewCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    private static readonly Dictionary<(ReviewStatus From, string Action), ReviewStatus> _transitions = new()
    {
        { (ReviewStatus.NotStarted, "start"), ReviewStatus.SelfInProgress },
        { (ReviewStatus.SelfSubmitted, "open-peer-review"), ReviewStatus.PeerReviewOpen },
        { (ReviewStatus.PeerReviewOpen, "close-peer-review"), ReviewStatus.PeerReviewDone },
        { (ReviewStatus.PeerReviewDone, "manager-review"), ReviewStatus.ManagerReview },
        { (ReviewStatus.ManagerSubmitted, "calibrate"), ReviewStatus.Calibration },
        { (ReviewStatus.Calibration, "finalize"), ReviewStatus.Finalized },
    };

    public TransitionReviewCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(TransitionReviewCommand request, CancellationToken ct)
    {
        var review = await _uow.PerformanceReviews.GetByIdAsync(request.ReviewId, ct);
        if (review is null)
            return Result<bool>.Failure("Review not found");

        var key = (review.Status, request.Action.ToLowerInvariant());
        if (!_transitions.TryGetValue(key, out var nextStatus))
            return Result<bool>.Failure($"Cannot perform '{request.Action}' from status '{review.Status}'");

        review.Status = nextStatus;
        review.UpdatedBy = _currentUser.Email;

        _uow.PerformanceReviews.Update(review);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PerformanceReviews(review.EmployeeId), ct);

        return Result<bool>.Success(true, $"Review transitioned to {nextStatus}");
    }
}
