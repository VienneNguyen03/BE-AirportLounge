using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Performance.Commands;

public record CompleteReviewCommand(
    Guid ReviewId,
    string ManagerAssessment,
    decimal OverallScore,
    string? ImprovementPlan) : IRequest<Result<bool>>;

public class CompleteReviewCommandHandler : IRequestHandler<CompleteReviewCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CompleteReviewCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(CompleteReviewCommand request, CancellationToken ct)
    {
        var review = await _uow.PerformanceReviews.GetByIdAsync(request.ReviewId, ct);
        if (review is null)
            return Result<bool>.Failure("Performance review not found");

        if (review.Status != ReviewStatus.ManagerReview)
            return Result<bool>.Failure("Review is not in Manager Review state");

        review.ManagerAssessment = request.ManagerAssessment;
        review.OverallScore = request.OverallScore;
        review.ImprovementPlan = request.ImprovementPlan;
        review.Status = ReviewStatus.Completed;
        review.ReviewDate = DateTime.UtcNow;
        review.UpdatedBy = _currentUser.Email;

        _uow.PerformanceReviews.Update(review);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PerformanceReviews(review.EmployeeId), ct);

        return Result<bool>.Success(true, "Review completed");
    }
}
