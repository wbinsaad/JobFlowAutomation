namespace JobFlowAutomation.Application.Models;

public sealed record JobApplicationTransitionResult(
    JobApplicationTransitionOutcome Outcome,
    JobApplicationRecord? Application,
    string? Message);
