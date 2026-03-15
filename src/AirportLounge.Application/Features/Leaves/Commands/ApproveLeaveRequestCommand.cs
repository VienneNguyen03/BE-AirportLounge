using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Approve: UnderReview → Approved (Manager) ────────────────────
public record ApproveLeaveRequestCommand(
    Guid LeaveRequestId,
    string? Comment) : IRequest<Result<bool>>;
