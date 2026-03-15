using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Create Draft ─────────────────────────────────────────────────
public record CreateLeaveRequestDraftCommand(
    Guid LeaveTypeId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsHalfDay,
    string? Reason) : IRequest<Result<Guid>>;
