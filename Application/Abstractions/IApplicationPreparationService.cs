using JobFlowAutomation.Application.Models;

namespace JobFlowAutomation.Application.Abstractions;

/// <summary>
/// Coordinates CV selection and CV file validation for a job.
/// </summary>
public interface IApplicationPreparationService
{
    /// <summary>
    /// Prepares a job application for manual review.
    /// </summary>
    /// <param name="request">
    /// The scraped job information required for preparation.
    /// </param>
    /// <returns>
    /// The preparation outcome.
    /// </returns>
    ApplicationPreparationResult Prepare(
        ApplicationPreparationRequest request);
}
