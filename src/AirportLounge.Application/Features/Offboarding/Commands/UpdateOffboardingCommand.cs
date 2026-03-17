using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Offboarding.Commands;

public record UpdateOffboardingCommand(
    Guid ProcessId,
    bool? ExitSurveyCompleted,
    bool? AssetReturned,
    bool? AccessRevoked) : IRequest<Result<bool>>;
