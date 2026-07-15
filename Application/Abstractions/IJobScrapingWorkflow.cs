using JobFlowAutomation.Domain;

namespace JobFlowAutomation.Application.Abstractions;

public interface IJobScrapingWorkflow
{
    Task<IReadOnlyList<JobScrapeResult>> RunAsync(
        string searchPageUrl,
        CancellationToken cancellationToken = default);
}
