using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Open for Review: Submitted → UnderReview (Manager only) ──────
public record OpenLeaveForReviewCommand(Guid LeaveRequestId) : IRequest<Result<bool>>;
