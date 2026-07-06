using JobFlowAutomation.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace JobFlowAutomation.Infrastructure.Seek;

public sealed class SeekJobDetailPageExtractor
{
    private readonly IWebDriver _driver;
    private readonly ILogger<SeekJobDetailPageExtractor> _logger;

    public SeekJobDetailPageExtractor(IWebDriver driver, ILogger<SeekJobDetailPageExtractor> logger)
    {
        _driver = driver;
        _logger = logger;
    }

    public ExtractionResult<JobDetail> Extract()
    {
        try
        {
            var title = _driver.FindElement(SeekSelectors.Detail.Title).Text.Trim();
            var description = _driver.FindElement(SeekSelectors.Detail.Description).Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                return ExtractionResult<JobDetail>.Failure(
                    new ExtractionIssue("Title", "Missing title", ExtractionIssueSeverity.Error));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return ExtractionResult<JobDetail>.Failure(
                    new ExtractionIssue("Description", "Missing description", ExtractionIssueSeverity.Error));
            }

            var company = TryFindText(SeekSelectors.Card.Company);
            var location = TryFindText(SeekSelectors.Card.Location);
            var classifications = TryFindText(SeekSelectors.Detail.Classifications);
            var workType = TryFindText(SeekSelectors.Detail.WorkType);
            var salary = TryFindText(SeekSelectors.Detail.Salary);
            var quickApplyText = TryFindText(SeekSelectors.Detail.IsQuickApply);

            return ExtractionResult<JobDetail>.Success(
                new JobDetail(
                    Title: title,
                    AdvertiserName: company,
                    Location: location,
                    Classifications: classifications,
                    Salary: salary,
                    WorkType: workType,
                    Description: description,
                    IsQuickApply: quickApplyText == "Quick apply"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract job detail page");

            return ExtractionResult<JobDetail>.Failure(
                new ExtractionIssue("Page", ex.Message, ExtractionIssueSeverity.Error));
        }
    }

    private string? TryFindText(By by)
    {
        try
        {
            return _driver.FindElement(by).Text?.Trim();
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }
}
