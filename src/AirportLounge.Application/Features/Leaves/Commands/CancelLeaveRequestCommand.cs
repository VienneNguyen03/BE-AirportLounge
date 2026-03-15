using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Cancel: Draft | Submitted | Approved | Scheduled → Cancelled ─
public record CancelLeaveRequestCommand(Guid LeaveRequestId) : IRequest<Result<bool>>;
