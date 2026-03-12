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
