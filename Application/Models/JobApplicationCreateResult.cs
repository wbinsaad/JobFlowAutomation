namespace JobFlowAutomation.Application.Models;

public sealed record JobApplicationCreateResult(
    JobApplicationRecord Application,
    bool WasCreated);
