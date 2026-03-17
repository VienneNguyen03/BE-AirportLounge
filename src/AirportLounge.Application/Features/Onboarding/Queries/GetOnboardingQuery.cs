using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

namespace AirportLounge.Application.Features.Onboarding.Queries;

public record OnboardingTaskDto(
    Guid Id, string Title, string? Description, string? AssignedTo,
    bool IsCompleted, DateTime? CompletedAt, DateTime? DueDate, int SortOrder,
    Guid? TaskCategoryId = null, string? TaskCategoryName = null);

public record OnboardingDto(
    Guid Id, Guid EmployeeId, string EmployeeName, OnboardingStatus Status,
    DateTime StartDate, DateTime? CompletedDate, string? MentorName,
    List<OnboardingTaskDto> Tasks);

public record GetOnboardingQuery(Guid EmployeeId) : IRequest<Result<OnboardingDto>>;
