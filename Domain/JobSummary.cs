namespace JobFlowAutomation.Domain;

public sealed record JobSummary(
    string Title,
    Uri DetailUrl,
    string? Company,
    string? Location);
