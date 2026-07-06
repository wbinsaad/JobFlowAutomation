namespace JobFlowAutomation.Domain;

public sealed record JobDetail(
    string Title,
    string? AdvertiserName,
    string? Location,
    string? Classifications,
    string? Salary,
    string? WorkType,
    string Description,
    bool IsQuickApply);
