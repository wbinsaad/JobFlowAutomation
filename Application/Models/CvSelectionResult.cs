namespace JobFlowAutomation.Application.Models;

/// <summary>
/// Describes the selected CV profile and the reasons it was selected.
/// </summary>
/// <param name="ProfileName">
/// The configured CV profile name.
/// </param>
/// <param name="FilePath">
/// The configured CV file path.
/// </param>
/// <param name="Score">
/// The calculated selection score.
/// </param>
/// <param name="MatchedTitleKeywords">
/// Title keywords found in the advertised job title.
/// </param>
/// <param name="MatchedDescriptionKeywords">
/// Description keywords found in the advertised job description.
/// </param>
/// <param name="RequiresManualApproval">
/// Indicates whether the selection requires manual approval.
/// </param>
public sealed record CvSelectionResult(
    string ProfileName,
    string FilePath,
    int Score,
    IReadOnlyList<string> MatchedTitleKeywords,
    IReadOnlyList<string> MatchedDescriptionKeywords,
    bool RequiresManualApproval);
