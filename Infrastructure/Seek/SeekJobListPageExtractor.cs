using JobFlowAutomation.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace JobFlowAutomation.Infrastructure.Seek;

public sealed class SeekJobListPageExtractor
{
    private readonly IWebDriver _driver;
    private readonly SeekJobCardExtractor _cardExtractor;
    private readonly ILogger<SeekJobListPageExtractor> _logger;

    public SeekJobListPageExtractor(
        IWebDriver driver,
        SeekJobCardExtractor cardExtractor,
        ILogger<SeekJobListPageExtractor> logger)
    {
        _driver = driver;
        _cardExtractor = cardExtractor;
        _logger = logger;
    }

    public IReadOnlyList<ExtractionResult<JobSummary>> Extract()
    {
        var cards = _driver.FindElements(SeekSelectors.JobCards);

        _logger.LogInformation("Found {CardCount} job cards", cards.Count);

        var results = new List<ExtractionResult<JobSummary>>();

        foreach (var card in cards)
        {
            var result = _cardExtractor.Extract(card);
            results.Add(result);
        }

        var successCount = results.Count(result => result.IsSuccess);
        var failCount = results.Count - successCount;

        _logger.LogInformation(
            "Job card extraction completed. Success={Success}, Fail={Fail}",
            successCount,
            failCount);

        return results;
    }
}
