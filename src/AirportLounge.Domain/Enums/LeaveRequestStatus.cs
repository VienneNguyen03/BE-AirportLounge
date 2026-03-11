namespace AirportLounge.Domain.Enums;

public enum LeaveRequestStatus
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    NeedsInfo = 3,
    Approved = 4,
    Scheduled = 5,
    Rejected = 6,
    Cancelled = 7,
    Taken = 8
}
