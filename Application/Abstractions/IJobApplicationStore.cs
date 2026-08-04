using JobFlowAutomation.Application.Models;

namespace JobFlowAutomation.Application.Abstractions;

public interface IJobApplicationStore
{
    Task<JobApplicationCreateResult>
        CreateOrGetAsync(
            JobApplicationDraft draft,
            CancellationToken cancellationToken =
                default);

    Task<JobApplicationRecord?>
        GetByCanonicalUrlAsync(
            Uri jobUrl,
            CancellationToken cancellationToken =
                default);

    Task<JobApplicationTransitionResult>
        TryTransitionAsync(
            Guid applicationId,
            JobApplicationStatus targetStatus,
            string? failureCode = null,
            string? failureMessage = null,
            CancellationToken cancellationToken =
                default);
}
