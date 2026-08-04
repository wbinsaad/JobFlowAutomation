using JobFlowAutomation.Application.Models;

namespace JobFlowAutomation.Application.Services;

public static class JobApplicationStatusPolicy
{
    public static bool CanTransition(
        JobApplicationStatus currentStatus,
        JobApplicationStatus targetStatus)
    {
        return currentStatus switch
        {
            JobApplicationStatus.AwaitingReview =>
                targetStatus is
                    JobApplicationStatus.Approved
                    or JobApplicationStatus.Skipped
                    or JobApplicationStatus.Rejected
                    or JobApplicationStatus.Failed,

            JobApplicationStatus.Approved =>
                targetStatus is
                    JobApplicationStatus.Prepared
                    or JobApplicationStatus.AwaitingReview
                    or JobApplicationStatus.Failed,

            JobApplicationStatus.Prepared =>
                targetStatus is
                    JobApplicationStatus.Submitted
                    or JobApplicationStatus.Failed,

            JobApplicationStatus.Failed =>
                targetStatus
                    == JobApplicationStatus.AwaitingReview,

            JobApplicationStatus.Skipped =>
                targetStatus
                    == JobApplicationStatus.AwaitingReview,

            JobApplicationStatus.Rejected =>
                targetStatus
                    == JobApplicationStatus.AwaitingReview,

            _ => false
        };
    }
}
