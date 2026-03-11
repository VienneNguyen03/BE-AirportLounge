namespace AirportLounge.Domain.Enums;

public enum ReviewStatus
{
    NotStarted = 0,
    SelfInProgress = 1,
    SelfSubmitted = 2,
    PeerReviewOpen = 3,
    PeerReviewDone = 4,
    ManagerReview = 5,
    ManagerSubmitted = 6,
    Calibration = 7,
    Finalized = 8
}
