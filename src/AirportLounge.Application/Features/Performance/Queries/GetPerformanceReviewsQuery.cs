using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Performance.Queries;

public record GetPerformanceReviewsQuery(Guid EmployeeId) : IRequest<Result<List<PerformanceReviewDto>>>;

public record ReviewFeedbackDto(
    Guid Id,
    Guid FromStaffId,
    string? Comment,
    decimal? Score,
    bool IsAnonymous,
    DateTime CreatedAt);

public record PerformanceReviewDto(
    Guid Id,
    Guid EmployeeId,
    Guid ReviewerId,
    string ReviewerName,
    string Period,
    ReviewType ReviewType,
    string? SelfAssessment,
    decimal? SelfScore,
    string? ManagerAssessment,
    decimal? ManagerScore,
    decimal? OverallScore,
    ReviewStatus Status,
    DateTime? ReviewDate,
    DateTime? DueDate,
    string? Comments,
    string? ImprovementPlan,
    List<ReviewFeedbackDto> Feedbacks,
    DateTime CreatedAt);

public class GetPerformanceReviewsQueryHandler : IRequestHandler<GetPerformanceReviewsQuery, Result<List<PerformanceReviewDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetPerformanceReviewsQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<List<PerformanceReviewDto>>> Handle(GetPerformanceReviewsQuery request, CancellationToken ct)
    {
        var cacheKey = CacheKeys.PerformanceReviews(request.EmployeeId);
        var cached = await _cache.GetAsync<List<PerformanceReviewDto>>(cacheKey, ct);
        if (cached is not null)
            return Result<List<PerformanceReviewDto>>.Success(cached);

        var reviews = await _uow.PerformanceReviews.Query()
            .Include(r => r.Reviewer)
            .Include(r => r.Feedbacks)
            .Where(r => r.EmployeeId == request.EmployeeId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new PerformanceReviewDto(
                r.Id, r.EmployeeId, r.ReviewerId, r.Reviewer.FullName,
                r.Period, r.ReviewType, r.SelfAssessment, r.SelfScore,
                r.ManagerAssessment, r.ManagerScore,
                r.OverallScore, r.Status, r.ReviewDate, r.DueDate,
                r.Comments, r.ImprovementPlan,
                r.Feedbacks.Select(f => new ReviewFeedbackDto(
                    f.Id, f.FromStaffId, f.Comment, f.Score, f.IsAnonymous, f.CreatedAt
                )).ToList(),
                r.CreatedAt))
            .ToListAsync(ct);

        await _cache.SetAsync(cacheKey, reviews, CacheKeys.PerformanceTtl, ct);
        return Result<List<PerformanceReviewDto>>.Success(reviews);
    }
}
