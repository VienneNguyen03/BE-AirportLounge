using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Request More Info: UnderReview → NeedsInfo (Manager) ─────────
public record RequestMoreInfoCommand(
    Guid LeaveRequestId,
    string Comment) : IRequest<Result<bool>>;
