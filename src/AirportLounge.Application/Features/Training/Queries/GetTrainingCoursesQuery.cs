using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Training.Queries;

public record GetTrainingCoursesQuery : IRequest<Result<List<TrainingCourseDto>>>;

public record TrainingCourseDto(
    Guid Id,
    string Title,
    string? Description,
    string? Category,
    int DurationInHours,
    string? ContentUrl,
    decimal PassingScore,
    bool IsActive,
    DateTime CreatedAt);

public class GetTrainingCoursesQueryHandler : IRequestHandler<GetTrainingCoursesQuery, Result<List<TrainingCourseDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetTrainingCoursesQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<List<TrainingCourseDto>>> Handle(GetTrainingCoursesQuery request, CancellationToken ct)
    {
        var cached = await _cache.GetAsync<List<TrainingCourseDto>>(CacheKeys.TrainingCoursesList, ct);
        if (cached is not null)
            return Result<List<TrainingCourseDto>>.Success(cached);

        var courses = await _uow.TrainingCourses.Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .Select(c => new TrainingCourseDto(
                c.Id, c.Title, c.Description, c.Category,
                c.DurationInHours, c.ContentUrl, c.PassingScore,
                c.IsActive, c.CreatedAt))
            .ToListAsync(ct);

        await _cache.SetAsync(CacheKeys.TrainingCoursesList, courses, CacheKeys.TrainingTtl, ct);
        return Result<List<TrainingCourseDto>>.Success(courses);
    }
}
