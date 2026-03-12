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
