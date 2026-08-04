namespace JobFlowAutomation.Application.Models;

/// <summary>
/// Contains the scraped job information required to prepare
/// a job application.
/// </summary>
/// <param name="JobTitle">
/// The advertised job title.
/// </param>
/// <param name="JobDescription">
/// The advertised description, or <see langword="null"/>
/// when no description is available.
/// </param>
/// <param name="Company">
/// The advertised company or recruiter name.
/// </param>
/// <param name="JobUrl">
/// The absolute URL of the job advertisement.
/// </param>
public sealed record ApplicationPreparationRequest(
    string JobTitle,
    string? JobDescription,
    string? Company,
    Uri JobUrl);
