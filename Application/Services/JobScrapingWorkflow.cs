using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Domain;

namespace JobFlowAutomation.Application.Services;

public sealed class JobScrapingWorkflow : IJobScrapingWorkflow
{
    private readonly IJobScraper _jobScraper;
    private readonly IJobScrapeStore _jobScrapeStore;

    public JobScrapingWorkflow(
        IJobScraper jobScraper,
        IJobScrapeStore jobScrapeStore)
    {
        _jobScraper = jobScraper;
        _jobScrapeStore = jobScrapeStore;
    }

    public async Task<IReadOnlyList<JobScrapeResult>> RunAsync(
        string searchPageUrl,
        CancellationToken cancellationToken = default)
    {
        var scrapeResults = await _jobScraper.ScrapeAsync(
            searchPageUrl,
            cancellationToken);

        await _jobScrapeStore.SaveScrapeResultsAsync(
            scrapeResults,
            cancellationToken);

        return scrapeResults;
    }
}