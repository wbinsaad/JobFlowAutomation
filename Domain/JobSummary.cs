namespace JobFlowAutomation.Domain;

public sealed record JobSummary(
    string Title,
    Uri DetailUrl,
    string? AdvertiserName,
    string? Location);
