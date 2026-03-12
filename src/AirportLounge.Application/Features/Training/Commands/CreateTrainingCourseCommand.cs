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
