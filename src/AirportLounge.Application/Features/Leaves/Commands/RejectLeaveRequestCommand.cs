using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Reject: UnderReview → Rejected (Manager) ─────────────────────
public record RejectLeaveRequestCommand(
    Guid LeaveRequestId,
    string? Reason) : IRequest<Result<bool>>;
