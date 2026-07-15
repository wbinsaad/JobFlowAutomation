using JobFlowAutomation.Domain;

namespace JobFlowAutomation.Application.Abstractions;

public interface IJobScraper
{
    IReadOnlyList<JobScrapeResult> Scrape(string url);

    Task<IReadOnlyList<JobScrapeResult>> ScrapeAsync(string url, CancellationToken cancellationToken = default);
}
