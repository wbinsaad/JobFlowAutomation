using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Configuration;
using JobFlowAutomation.Domain;
using JobFlowAutomation.Infrastructure.Selenium;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobFlowAutomation.Infrastructure.Seek;

public sealed class SeekScraper : IJobScraper
{
    private readonly SeleniumNavigator _navigator;
    private readonly SeekJobListPageExtractor _listExtractor;
    private readonly SeekJobDetailPageExtractor _detailExtractor;
    private readonly SeekScraperOptions _options;
    private readonly ILogger<SeekScraper> _logger;

    public SeekScraper(
        SeleniumNavigator navigator,
        SeekJobListPageExtractor listExtractor,
        SeekJobDetailPageExtractor detailExtractor,
        IOptions<SeekScraperOptions> options,
        ILogger<SeekScraper> logger)
    {
        _navigator = navigator;
        _listExtractor = listExtractor;
        _detailExtractor = detailExtractor;
        _options = options.Value;
        _logger = logger;
    }

    public IReadOnlyList<JobScrapeResult> Scrape(string url) =>
        ScrapeAsync(url).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<JobScrapeResult>> ScrapeAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seek scraping started");

        _navigator.NavigateTo(url, SeekSelectors.JobCards);

        var summaries = _listExtractor.Extract()
            .Where(result => result.IsSuccessful && result.Value is not null)
            .Select(result => result.Value!)
            .ToList();

        _logger.LogInformation("Extracted {JobCount} valid job summaries", summaries.Count);

        var results = new List<JobScrapeResult>();

        foreach (var job in summaries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _logger.LogInformation(
                    "Opening detail page for {Title} at {Company}",
                    job.Title,
                    job.AdvertiserName);

                _navigator.NavigateTo(job.DetailUrl.ToString(), SeekSelectors.Detail.Title);

                var delayMs = Random.Shared.Next(_options.MinDelayMs, _options.MaxDelayMs + 1);

                _logger.LogDebug("Waiting {DelayMs} ms before extracting details", delayMs);
                await Task.Delay(delayMs, cancellationToken);

                var detail = _detailExtractor.Extract();
                results.Add(new JobScrapeResult(job, detail));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Scraping cancelled while processing {Title}", job.Title);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed while processing detail page for {Title} at {Url}",
                    job.Title,
                    job.DetailUrl);

                results.Add(new JobScrapeResult(
                    job,
                    ExtractionResult<JobDetail>.Failure(
                        new ExtractionIssue("Navigation", ex.Message, ExtractionIssueSeverity.Error))));
            }
        }

        _logger.LogInformation("Seek scraping finished. ResultCount={ResultCount}", results.Count);

        return results;
    }
}
