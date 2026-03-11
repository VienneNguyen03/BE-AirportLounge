using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Performance.Commands;

public record AddPeerFeedbackCommand(
    Guid ReviewId,
    string? Comment,
    decimal? Score,
    bool IsAnonymous) : IRequest<Result<Guid>>;

public class AddPeerFeedbackCommandHandler : IRequestHandler<AddPeerFeedbackCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public AddPeerFeedbackCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(AddPeerFeedbackCommand request, CancellationToken ct)
    {
        var review = await _uow.PerformanceReviews.GetByIdAsync(request.ReviewId, ct);
        if (review is null)
            return Result<Guid>.Failure("Review not found");

        if (review.Status != ReviewStatus.PeerReviewOpen)
            return Result<Guid>.Failure("Peer review is not open");

        if (_currentUser.UserId is null)
            return Result<Guid>.Failure("Not authenticated");

        var feedback = new ReviewFeedback
        {
            ReviewId = request.ReviewId,
            FromStaffId = _currentUser.UserId.Value,
            Comment = request.Comment,
            Score = request.Score,
            IsAnonymous = request.IsAnonymous,
            CreatedBy = _currentUser.Email
        };

        await _uow.ReviewFeedbacks.AddAsync(feedback, ct);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PerformanceReviews(review.EmployeeId), ct);

        return Result<Guid>.Success(feedback.Id, "Peer feedback added");
    }
}
