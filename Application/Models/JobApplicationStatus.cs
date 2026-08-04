namespace JobFlowAutomation.Application.Models;

public enum JobApplicationStatus
{
    Unknown = 0,
    AwaitingReview = 1,
    Approved = 2,
    Prepared = 3,
    Submitted = 4,
    Skipped = 5,
    Rejected = 6,
    Failed = 7
}
