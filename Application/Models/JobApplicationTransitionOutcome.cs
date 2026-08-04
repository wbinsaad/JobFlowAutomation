namespace JobFlowAutomation.Application.Models;

public enum JobApplicationTransitionOutcome
{
    Updated = 0,
    NotFound = 1,
    InvalidTransition = 2,
    InvalidRequest = 3,
    ConcurrencyConflict = 4
}
