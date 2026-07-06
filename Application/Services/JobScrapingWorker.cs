using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;

namespace JobFlowAutomation.Application.Services;

public sealed class JobScrapingWorker : BackgroundService
{
    private readonly IJobScraper _scraper;
    private readonly ScraperOptions _options;
    private readonly IWebDriver _driver;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<JobScrapingWorker> _logger;

    public JobScrapingWorker(
        IJobScraper scraper,
        IOptions<ScraperOptions> options,
        IWebDriver driver,
        IHostApplicationLifetime lifetime,
        ILogger<JobScrapingWorker> logger)
    {
        _scraper = scraper;
        _options = options.Value;
        _driver = driver;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Application started");
            _logger.LogInformation("Starting scrape for URL: {Url}", _options.Url);

            var results = await _scraper.ScrapeAsync(_options.Url, stoppingToken);

            var successCount = 0;
            var failCount = 0;

            foreach (var result in results)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Scraping stopped by cancellation request");
                    break;
                }

                _logger.LogInformation(
                    "Processing job: {Title} at {Company}",
                    result.Summary.Title,
                    result.Summary.Company);

                if (result.Detail.IsSuccess && result.Detail.Value is not null)
                {
                    successCount++;

                    _logger.LogInformation(
                        "Detail scrape success for {Title}",
                        result.Summary.Title);

                    var previewLength = Math.Min(200, result.Detail.Value.Description.Length);

                    _logger.LogDebug(
                        "Description preview: {Preview}",
                        result.Detail.Value.Description[..previewLength]);
                }
                else
                {
                    failCount++;

                    _logger.LogWarning(
                        "Detail scrape failed for {Title}",
                        result.Summary.Title);

                    foreach (var issue in result.Detail.Issues)
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
                failCount);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Job scraping worker cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job scraping worker failed");
        }
        finally
        {
            CloseBrowserSafely();

            if (_options.WaitForKeyBeforeExit)
            {
                _logger.LogInformation("Press any key to exit");
                Console.ReadKey(intercept: true);
            }

            _logger.LogInformation("Application finished");
            _lifetime.StopApplication();
        }
    }

    private void CloseBrowserSafely()
    {
        try
        {
            _logger.LogInformation("Closing browser");
            _driver.Quit();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Browser was already closed or could not be closed cleanly");
        }
    }
}
