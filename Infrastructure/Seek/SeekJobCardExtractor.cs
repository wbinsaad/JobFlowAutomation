using JobFlowAutomation.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace JobFlowAutomation.Infrastructure.Seek;

public sealed class SeekJobCardExtractor
{
    private readonly ILogger<SeekJobCardExtractor> _logger;

    public SeekJobCardExtractor(ILogger<SeekJobCardExtractor> logger)
    {
        _logger = logger;
    }

    public ExtractionResult<JobSummary> Extract(IWebElement card)
    {
        try
        {
            var titleElement = SafeFind(card, SeekSelectors.Card.Title);

            if (titleElement is null)
            {
                return ExtractionResult<JobSummary>.Failure(
                    new ExtractionIssue("Title", "Missing job title", ExtractionIssueSeverity.Error));
            }

            var title = titleElement.Text?.Trim();
            var href = titleElement.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(title))
            {
                return ExtractionResult<JobSummary>.Failure(
                    new ExtractionIssue("Title", "Empty job title", ExtractionIssueSeverity.Error));
            }

            if (string.IsNullOrWhiteSpace(href))
            {
                return ExtractionResult<JobSummary>.Failure(
                    new ExtractionIssue("DetailUrl", "Missing job URL", ExtractionIssueSeverity.Error));
            }

            var company = SafeFind(card, SeekSelectors.Card.Company)?.Text?.Trim();
            var location = SafeFind(card, SeekSelectors.Card.Location)?.Text?.Trim();

            return ExtractionResult<JobSummary>.Success(
                new JobSummary(
                    Title: title,
                    DetailUrl: new Uri(href),
                    Company: company,
                    Location: location));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract job card");

            return ExtractionResult<JobSummary>.Failure(
                new ExtractionIssue("Card", ex.Message, ExtractionIssueSeverity.Error));
        }
    }

    private static IWebElement? SafeFind(IWebElement parent, By by)
    {
        try
        {
            return parent.FindElement(by);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }
}
