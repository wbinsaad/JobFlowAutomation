using JobFlowAutomation.Domain;

namespace JobFlowAutomation.UnitTests.Domain;

public sealed class ExtractionResultTests
{
    [Fact]
    public void Success_WhenValueIsProvided_CreatesSuccessfulResult()
    {
        // Arrange
        var jobSummary = new JobSummary(
            Title: "Software Engineer",
            DetailUrl: new Uri("https://www.seek.com.au/job/12345"),
            AdvertiserName: "Example Company",
            Location: "Melbourne VIC");

        // Act
        var result = ExtractionResult<JobSummary>.Success(jobSummary);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Same(jobSummary, result.Value);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Failure_WhenIssueIsProvided_CreatesFailedResult()
    {
        // Arrange
        var extractionIssue = new ExtractionIssue(
            Field: "Title",
            Message: "Missing job title",
            Severity: ExtractionIssueSeverity.Error);

        // Act
        var result =
            ExtractionResult<JobSummary>.Failure(extractionIssue);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Null(result.Value);

        var recordedIssue = Assert.Single(result.Issues);

        Assert.Same(extractionIssue, recordedIssue);
    }
}
