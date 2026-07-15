namespace JobFlowAutomation.Domain;

public sealed record ExtractionResult<T>(
    T? Value,
    bool IsSuccessful,
    IReadOnlyList<ExtractionIssue> Issues)
{
    public static ExtractionResult<T> Success(T value) =>
        new(value, true, Array.Empty<ExtractionIssue>());

    public static ExtractionResult<T> Failure(params ExtractionIssue[] issues) =>
        new(default, false, issues);
}

public sealed record ExtractionIssue(
    string Field,
    string Message,
    ExtractionIssueSeverity Severity);

public enum ExtractionIssueSeverity
{
    Warning,
    Error
}
