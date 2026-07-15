using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Configuration;
using JobFlowAutomation.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobFlowAutomation.Hosting.Workers;

public sealed class JobScrapingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SeekScraperOptions _options;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<JobScrapingWorker> _logger;

    public JobScrapingWorker(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<SeekScraperOptions> options,
        IHostApplicationLifetime applicationLifetime,
        ILogger<JobScrapingWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Application started");
            _logger.LogInformation(
                "Starting scrape for URL: {Url}",
                _options.SearchPageUrl);

            IReadOnlyList<JobScrapeResult> scrapeResults;

            await using (var scope =
                _serviceScopeFactory.CreateAsyncScope())
            {
                var jobScrapingWorkflow =
                    scope.ServiceProvider
                        .GetRequiredService<IJobScrapingWorkflow>();

                scrapeResults = await jobScrapingWorkflow.RunAsync(
                    _options.SearchPageUrl,
                    stoppingToken);
            }

            LogScrapeResults(scrapeResults, stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Job scraping worker cancelled");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Job scraping worker failed");
        }
        finally
        {
            if (_options.WaitForKeyBeforeExit)
            {
                _logger.LogInformation("Press any key to exit");
                Console.ReadKey(intercept: true);
            }

            _logger.LogInformation("Application finished");
            _applicationLifetime.StopApplication();
        }
    }

    private void LogScrapeResults(
        IReadOnlyList<JobScrapeResult> scrapeResults,
        CancellationToken stoppingToken)
    {
        var successCount = 0;
        var failureCount = 0;

        foreach (var scrapeResult in scrapeResults)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Scraping stopped by cancellation request");
                break;
            }

            _logger.LogInformation(
                "Processing job: {Title} at {Company}",
                scrapeResult.Summary.Title,
                scrapeResult.Summary.AdvertiserName);

            if (scrapeResult.Detail.IsSuccessful &&
                scrapeResult.Detail.Value is not null)
            {
                successCount++;

                _logger.LogInformation(
                    "Detail scrape success for {Title}",
                    scrapeResult.Summary.Title);

                var description =
                    scrapeResult.Detail.Value.Description;

                var previewLength = Math.Min(
                    200,
                    description.Length);

                _logger.LogDebug(
                    "Description preview: {Preview}",
                    description[..previewLength]);
            }
            else
            {
                failureCount++;

                _logger.LogWarning(
                    "Detail scrape failed for {Title}",
                    scrapeResult.Summary.Title);

                foreach (var issue in scrapeResult.Detail.Issues)
                {
                    _logger.LogError(
                        "Field: {Field} | Severity: {Severity} | Message: {Message}",
                        issue.Field,
                        issue.Severity,
                        issue.Message);
                }
            }
        }

        _logger.LogInformation(
            "Scraping completed. Success={Success}, Fail={Fail}",
            successCount,
            failureCount);
    }
}
