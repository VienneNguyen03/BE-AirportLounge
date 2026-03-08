using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Performance.Commands;

public record SubmitSelfAssessmentCommand(
    Guid ReviewId,
    string SelfAssessment) : IRequest<Result<bool>>;

public class SubmitSelfAssessmentCommandHandler : IRequestHandler<SubmitSelfAssessmentCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public SubmitSelfAssessmentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(SubmitSelfAssessmentCommand request, CancellationToken ct)
    {
        var review = await _uow.PerformanceReviews.GetByIdAsync(request.ReviewId, ct);
        if (review is null)
            return Result<bool>.Failure("Performance review not found");

        if (review.Status != ReviewStatus.Draft && review.Status != ReviewStatus.SelfAssessment)
            return Result<bool>.Failure("Review is not in a valid state for self-assessment");

        review.SelfAssessment = request.SelfAssessment;
        review.Status = ReviewStatus.ManagerReview;
        review.UpdatedBy = _currentUser.Email;

        _uow.PerformanceReviews.Update(review);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PerformanceReviews(review.EmployeeId), ct);

        return Result<bool>.Success(true, "Self-assessment submitted");
    }
}
