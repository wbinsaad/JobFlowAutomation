using JobFlowAutomation.Application.Models;

namespace JobFlowAutomation.Application.Abstractions;

/// <summary>
/// Selects the most appropriate configured CV profile for a job.
/// </summary>
public interface ICvSelector
{
    /// <summary>
    /// Selects a CV using the job title and optional description.
    /// </summary>
    /// <param name="jobTitle">
    /// The advertised job title.
    /// </param>
    /// <param name="jobDescription">
    /// The advertised job description, or <see langword="null"/>
    /// when no description is available.
    /// </param>
    /// <returns>
    /// The best matching CV selection, or <see langword="null"/>
    /// when CV selection is disabled or no title keyword matches.
    /// </returns>
    CvSelectionResult? Select(
        string jobTitle,
        string? jobDescription);
}
