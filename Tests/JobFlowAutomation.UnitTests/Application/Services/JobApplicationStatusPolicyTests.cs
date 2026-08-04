using JobFlowAutomation.Application.Models;
using JobFlowAutomation.Application.Services;

namespace JobFlowAutomation.UnitTests.Application.Services;

public sealed class JobApplicationStatusPolicyTests
{
    [Theory]
    [InlineData(
        JobApplicationStatus.AwaitingReview,
        JobApplicationStatus.Approved)]
    [InlineData(
        JobApplicationStatus.AwaitingReview,
        JobApplicationStatus.Skipped)]
    [InlineData(
        JobApplicationStatus.Approved,
        JobApplicationStatus.Prepared)]
    [InlineData(
        JobApplicationStatus.Prepared,
        JobApplicationStatus.Submitted)]
    [InlineData(
        JobApplicationStatus.Failed,
        JobApplicationStatus.AwaitingReview)]
    public void CanTransition_WhenTransitionIsAllowed_ReturnsTrue(
        JobApplicationStatus currentStatus,
        JobApplicationStatus targetStatus)
    {
        bool result =
            JobApplicationStatusPolicy.CanTransition(
                currentStatus,
                targetStatus);

        Assert.True(result);
    }

    [Theory]
    [InlineData(
        JobApplicationStatus.Submitted,
        JobApplicationStatus.AwaitingReview)]
    [InlineData(
        JobApplicationStatus.AwaitingReview,
        JobApplicationStatus.Submitted)]
    [InlineData(
        JobApplicationStatus.Skipped,
        JobApplicationStatus.Approved)]
    public void CanTransition_WhenTransitionIsNotAllowed_ReturnsFalse(
        JobApplicationStatus currentStatus,
        JobApplicationStatus targetStatus)
    {
        bool result =
            JobApplicationStatusPolicy.CanTransition(
                currentStatus,
                targetStatus);

        Assert.False(result);
    }
}
