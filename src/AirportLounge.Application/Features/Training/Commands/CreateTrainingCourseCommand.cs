using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Training.Commands;

public record CreateTrainingCourseCommand(
    string Title,
    string? Description,
    string? Category,
    int DurationInHours,
    string? ContentUrl,
    decimal PassingScore) : IRequest<Result<Guid>>;

public class CreateTrainingCourseCommandHandler : IRequestHandler<CreateTrainingCourseCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateTrainingCourseCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateTrainingCourseCommand request, CancellationToken ct)
    {
        var course = new TrainingCourse
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            DurationInHours = request.DurationInHours,
            ContentUrl = request.ContentUrl,
            PassingScore = request.PassingScore,
            IsActive = true,
            CreatedBy = _currentUser.Email
        };

        await _uow.TrainingCourses.AddAsync(course, ct);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.TrainingCoursesList, ct);

        return Result<Guid>.Success(course.Id, "Training course created");
    }
}
