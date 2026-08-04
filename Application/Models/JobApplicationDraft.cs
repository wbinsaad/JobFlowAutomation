namespace JobFlowAutomation.Application.Models;

public sealed record JobApplicationDraft
{
    private JobApplicationDraft(
        Uri jobUrl,
        string jobTitle,
        string? company,
        string selectedCvProfile,
        string selectedCvFileName,
        int selectionScore,
        IReadOnlyList<string> matchedTitleKeywords,
        IReadOnlyList<string> matchedDescriptionKeywords,
        bool requiresManualApproval)
    {
        JobUrl = jobUrl;
        JobTitle = jobTitle;
        Company = company;
        SelectedCvProfile = selectedCvProfile;
        SelectedCvFileName = selectedCvFileName;
        SelectionScore = selectionScore;
        MatchedTitleKeywords = matchedTitleKeywords;
        MatchedDescriptionKeywords =
            matchedDescriptionKeywords;
        RequiresManualApproval =
            requiresManualApproval;
    }

    public Uri JobUrl
    {
        get;
    }

    public string JobTitle
    {
        get;
    }

    public string? Company
    {
        get;
    }

    public string SelectedCvProfile
    {
        get;
    }

    public string SelectedCvFileName
    {
        get;
    }

    public int SelectionScore
    {
        get;
    }

    public IReadOnlyList<string> MatchedTitleKeywords
    {
        get;
    }

    public IReadOnlyList<string>
        MatchedDescriptionKeywords
    {
        get;
    }

    public bool RequiresManualApproval
    {
        get;
    }

    public static JobApplicationDraft FromPreparation(
        ApplicationPreparationRequest request,
        ApplicationPreparationResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);

        if (result.Status
            != ApplicationPreparationStatus.AwaitingReview)
        {
            throw new ArgumentException(
                "Only an awaiting-review preparation "
                + "can be persisted as an application.",
                nameof(result));
        }

        CvSelectionResult cvSelection =
            result.CvSelection
            ?? throw new ArgumentException(
                "The preparation result does not contain "
                + "a CV selection.",
                nameof(result));

        string cvFileName = GetFileName(
            cvSelection.FilePath);

        if (string.IsNullOrWhiteSpace(cvFileName))
        {
            throw new ArgumentException(
                "The selected CV path does not contain "
                + "a valid filename.",
                nameof(result));
        }

        return new JobApplicationDraft(
            request.JobUrl,
            request.JobTitle.Trim(),
            string.IsNullOrWhiteSpace(request.Company)
                ? null
                : request.Company.Trim(),
            cvSelection.ProfileName,
            cvFileName,
            cvSelection.Score,
            CopyKeywords(
                cvSelection.MatchedTitleKeywords),
            CopyKeywords(
                cvSelection
                    .MatchedDescriptionKeywords),
            cvSelection.RequiresManualApproval);
    }

    private static IReadOnlyList<string> CopyKeywords(
        IReadOnlyList<string> keywords)
    {
        return Array.AsReadOnly(
            keywords.ToArray());
    }

    private static string GetFileName(
        string filePath)
    {
        string platformPath = filePath
            .Replace(
                '\\',
                Path.DirectorySeparatorChar)
            .Replace(
                '/',
                Path.DirectorySeparatorChar);

        return Path.GetFileName(platformPath);
    }
}
