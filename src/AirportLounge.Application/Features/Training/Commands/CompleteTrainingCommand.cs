using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Training.Commands;

public record CompleteTrainingCommand(
    Guid EnrollmentId,
    decimal Score,
    string? CertificateUrl) : IRequest<Result<bool>>;
