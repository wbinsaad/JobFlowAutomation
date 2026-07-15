using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Application.Services;
using JobFlowAutomation.Domain;

namespace JobFlowAutomation.UnitTests.Application.Services;

public sealed class JobScrapingWorkflowTests
{
    [Fact]
    public async Task RunAsync_WhenScraperReturnsResults_SavesAndReturnsResultsAsync()
    {
        // Arrange
        const string SearchPageUrl =
            "https://www.seek.com.au/jobs/in-Melbourne-VIC";

        IReadOnlyList<JobScrapeResult> expectedResults =
            new[]
            {
                CreateSuccessfulScrapeResult()
            };

        var jobScraper = new StubJobScraper(expectedResults);
        var jobScrapeStore = new RecordingJobScrapeStore();

        var workflow = new JobScrapingWorkflow(
            jobScraper,
            jobScrapeStore);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        // Act
        var actualResults = await workflow.RunAsync(
            SearchPageUrl,
            cancellationTokenSource.Token);

        // Assert
        Assert.Same(expectedResults, actualResults);
        Assert.Same(expectedResults, jobScrapeStore.SavedResults);

        Assert.Equal(
            SearchPageUrl,
            jobScraper.ReceivedUrl);

        Assert.Equal(
            cancellationTokenSource.Token,
            jobScraper.ReceivedCancellationToken);

        Assert.Equal(
            cancellationTokenSource.Token,
            jobScrapeStore.ReceivedCancellationToken);
    }

    private static JobScrapeResult CreateSuccessfulScrapeResult()
    {
        var jobSummary = new JobSummary(
            Title: "Software Engineer",
            DetailUrl: new Uri("https://www.seek.com.au/job/12345"),
            AdvertiserName: "Example Company",
            Location: "Melbourne VIC");

        var jobDetail = new JobDetail(
            Title: "Software Engineer",
            AdvertiserName: "Example Company",
            Location: "Melbourne VIC",
            Classifications: "Information Technology",
            Salary: "$100,000",
            WorkType: "Full time",
            Description: "Example job description",
            IsQuickApply: true);

        return new JobScrapeResult(
            jobSummary,
            ExtractionResult<JobDetail>.Success(jobDetail));
    }

    private sealed class StubJobScraper : IJobScraper
    {
        private readonly IReadOnlyList<JobScrapeResult> _scrapeResults;

        public StubJobScraper(
            IReadOnlyList<JobScrapeResult> scrapeResults)
        {
            _scrapeResults = scrapeResults;
        }

        public string? ReceivedUrl
        {
            get; private set;
        }

        public CancellationToken ReceivedCancellationToken
        {
            get;
            private set;
        }

        public IReadOnlyList<JobScrapeResult> Scrape(string url)
        {
            ReceivedUrl = url;
            return _scrapeResults;
        }

        public Task<IReadOnlyList<JobScrapeResult>> ScrapeAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            ReceivedUrl = url;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(_scrapeResults);
        }
    }

    private sealed class RecordingJobScrapeStore : IJobScrapeStore
    {
        public IReadOnlyList<JobScrapeResult>? SavedResults
        {
            get;
            private set;
        }

        public CancellationToken ReceivedCancellationToken
        {
            get;
            private set;
        }

        public Task SaveScrapeResultsAsync(
            IReadOnlyList<JobScrapeResult> scrapeResults,
            CancellationToken cancellationToken = default)
        {
            SavedResults = scrapeResults;
            ReceivedCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}
