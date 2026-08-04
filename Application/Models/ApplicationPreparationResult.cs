namespace JobFlowAutomation.Application.Models;

/// <summary>
/// Represents the result of preparing a job application
/// for manual review.
/// </summary>
public sealed record ApplicationPreparationResult
{
    private ApplicationPreparationResult(
        ApplicationPreparationStatus status,
        CvSelectionResult? cvSelection,
        CvFileValidationResult? cvFileValidation,
        string message)
    {
        Status = status;
        CvSelection = cvSelection;
        CvFileValidation = cvFileValidation;
        Message = message;
    }

    public ApplicationPreparationStatus Status
    {
        get;
    }

    public CvSelectionResult? CvSelection
    {
        get;
    }

    public CvFileValidationResult? CvFileValidation
    {
        get;
    }

    public string Message
    {
        get;
    }

    public bool RequiresManualReview =>
        Status == ApplicationPreparationStatus.AwaitingReview;

    public static ApplicationPreparationResult InvalidJobData(
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            message);

        return new ApplicationPreparationResult(
            ApplicationPreparationStatus.InvalidJobData,
            cvSelection: null,
            cvFileValidation: null,
            message);
    }

    public static ApplicationPreparationResult NoMatchingCv()
    {
        return new ApplicationPreparationResult(
            ApplicationPreparationStatus.NoMatchingCv,
            cvSelection: null,
            cvFileValidation: null,
            "No configured CV profile matches the job title.");
    }

    public static ApplicationPreparationResult InvalidCvFile(
        CvSelectionResult cvSelection,
        CvFileValidationResult cvFileValidation)
    {
        ArgumentNullException.ThrowIfNull(
            cvSelection);

        ArgumentNullException.ThrowIfNull(
            cvFileValidation);

        if (cvFileValidation.IsValid)
        {
            throw new ArgumentException(
                "An invalid-CV result requires failed file validation.",
                nameof(cvFileValidation));
        }

        string message =
            cvFileValidation.ErrorMessage
            ?? "The selected CV file is invalid.";

        return new ApplicationPreparationResult(
            ApplicationPreparationStatus.InvalidCvFile,
            cvSelection,
            cvFileValidation,
            message);
    }

    public static ApplicationPreparationResult AwaitingReview(
        CvSelectionResult cvSelection,
        CvFileValidationResult cvFileValidation)
    {
        ArgumentNullException.ThrowIfNull(
            cvSelection);

        ArgumentNullException.ThrowIfNull(
            cvFileValidation);

        if (!cvFileValidation.IsValid)
        {
            throw new ArgumentException(
                "An awaiting-review result requires valid file validation.",
                nameof(cvFileValidation));
        }

        if (!cvSelection.RequiresManualApproval)
        {
            throw new ArgumentException(
                "The selected CV must require manual approval.",
                nameof(cvSelection));
        }

        return new ApplicationPreparationResult(
            ApplicationPreparationStatus.AwaitingReview,
            cvSelection,
            cvFileValidation,
            "Application preparation completed and requires manual review.");
    }
}
