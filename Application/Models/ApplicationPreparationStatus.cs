namespace JobFlowAutomation.Application.Models;

/// <summary>
/// Describes the outcome of preparing a job application.
/// </summary>
public enum ApplicationPreparationStatus
{
    /// <summary>
    /// No preparation outcome has been assigned.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The required job information is invalid.
    /// </summary>
    InvalidJobData = 1,

    /// <summary>
    /// No configured CV profile matches the job.
    /// </summary>
    NoMatchingCv = 2,

    /// <summary>
    /// A CV matched, but its configured file is invalid.
    /// </summary>
    InvalidCvFile = 3,

    /// <summary>
    /// Preparation succeeded and requires manual review.
    /// </summary>
    AwaitingReview = 4
}
