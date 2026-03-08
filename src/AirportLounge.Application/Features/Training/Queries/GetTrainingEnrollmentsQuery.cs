using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Training.Queries;

public record GetTrainingEnrollmentsQuery(Guid EmployeeId) : IRequest<Result<List<TrainingEnrollmentDto>>>;

public record TrainingEnrollmentDto(
    Guid Id,
    Guid EmployeeId,
    Guid CourseId,
    string CourseTitle,
    EnrollmentStatus Status,
    DateTime EnrolledAt,
    DateTime? CompletedAt,
    decimal? Score,
    string? CertificateUrl);

public class GetTrainingEnrollmentsQueryHandler : IRequestHandler<GetTrainingEnrollmentsQuery, Result<List<TrainingEnrollmentDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetTrainingEnrollmentsQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<List<TrainingEnrollmentDto>>> Handle(GetTrainingEnrollmentsQuery request, CancellationToken ct)
    {
        var cacheKey = CacheKeys.TrainingEnrollments(request.EmployeeId);
        var cached = await _cache.GetAsync<List<TrainingEnrollmentDto>>(cacheKey, ct);
        if (cached is not null)
            return Result<List<TrainingEnrollmentDto>>.Success(cached);

        var enrollments = await _uow.TrainingEnrollments.Query()
            .Include(e => e.Course)
            .Where(e => e.EmployeeId == request.EmployeeId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new TrainingEnrollmentDto(
                e.Id, e.EmployeeId, e.CourseId, e.Course.Title,
                e.Status, e.EnrolledAt, e.CompletedAt,
                e.Score, e.CertificateUrl))
            .ToListAsync(ct);

        await _cache.SetAsync(cacheKey, enrollments, CacheKeys.TrainingTtl, ct);
        return Result<List<TrainingEnrollmentDto>>.Success(enrollments);
    }
}
