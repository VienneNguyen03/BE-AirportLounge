using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Performance.Commands;

public record CreatePerformanceReviewCommand(
    Guid EmployeeId,
    string Period,
    ReviewType ReviewType,
    string? Comments) : IRequest<Result<Guid>>;

public class CreatePerformanceReviewCommandHandler : IRequestHandler<CreatePerformanceReviewCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreatePerformanceReviewCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreatePerformanceReviewCommand request, CancellationToken ct)
    {
        var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct);
        if (employee is null)
            return Result<Guid>.Failure("Employee not found");

        if (_currentUser.UserId is null)
            return Result<Guid>.Failure("Reviewer not authenticated");

        var review = new PerformanceReview
        {
            EmployeeId = request.EmployeeId,
            ReviewerId = _currentUser.UserId.Value,
            Period = request.Period,
            ReviewType = request.ReviewType,
            Status = ReviewStatus.Draft,
            Comments = request.Comments,
            ReviewDate = DateTime.UtcNow,
            CreatedBy = _currentUser.Email
        };

        await _uow.PerformanceReviews.AddAsync(review, ct);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PerformanceReviews(request.EmployeeId), ct);

        return Result<Guid>.Success(review.Id, "Performance review created");
    }
}
