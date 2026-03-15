using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Submit: Draft | NeedsInfo → Submitted ────────────────────────
public record SubmitLeaveRequestCommand(Guid LeaveRequestId) : IRequest<Result<bool>>;
