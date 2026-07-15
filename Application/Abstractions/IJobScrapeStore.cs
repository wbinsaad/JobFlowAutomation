using JobFlowAutomation.Domain;

namespace JobFlowAutomation.Application.Abstractions;

public interface IJobScrapeStore
{
    Task SaveScrapeResultsAsync(IReadOnlyList<JobScrapeResult> scrapeResults, CancellationToken cancellationToken = default);
}
