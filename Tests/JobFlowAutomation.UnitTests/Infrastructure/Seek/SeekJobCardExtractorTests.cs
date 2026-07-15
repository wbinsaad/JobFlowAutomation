using JobFlowAutomation.Domain;
using JobFlowAutomation.Infrastructure.Seek;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenQA.Selenium;

namespace JobFlowAutomation.UnitTests.Infrastructure.Seek;

public sealed class SeekJobCardExtractorTests
{
    [Fact]
    public void Extract_WhenCardContainsValidData_ReturnsSuccessfulJobSummary()
    {
        // Arrange
        const string ExpectedTitle = "Software Engineer";
        const string ExpectedDetailUrl = "https://www.seek.com.au/job/12345";
        const string ExpectedAdvertiserName = "Example Company";
        const string ExpectedLocation = "Melbourne VIC";

        var jobCardElement = Substitute.For<IWebElement>();
        var titleElement = Substitute.For<IWebElement>();
        var advertiserElement = Substitute.For<IWebElement>();
        var locationElement = Substitute.For<IWebElement>();

        titleElement.Text.Returns(
            $"  {ExpectedTitle}  ");

        titleElement
            .GetAttribute("href")
            .Returns(ExpectedDetailUrl);

        advertiserElement.Text.Returns(
            $"  {ExpectedAdvertiserName}  ");

        locationElement.Text.Returns(
            $"  {ExpectedLocation}  ");

        jobCardElement
            .FindElement(SeekSelectors.Card.Title)
            .Returns(titleElement);

        jobCardElement
            .FindElement(SeekSelectors.Card.Company)
            .Returns(advertiserElement);

        jobCardElement
            .FindElement(SeekSelectors.Card.Location)
            .Returns(locationElement);

        var logger = Substitute.For<ILogger<SeekJobCardExtractor>>();

        var extractor =
            new SeekJobCardExtractor(logger);

        // Act
        var extractionResult =
            extractor.Extract(jobCardElement);

        // Assert
        Assert.True(extractionResult.IsSuccessful);
        Assert.Empty(extractionResult.Issues);

        var jobSummary =
            Assert.IsType<JobSummary>(extractionResult.Value);

        Assert.Equal(
            ExpectedTitle,
            jobSummary.Title);

        Assert.Equal(
            new Uri(ExpectedDetailUrl),
            jobSummary.DetailUrl);

        Assert.Equal(
            ExpectedAdvertiserName,
            jobSummary.AdvertiserName);

        Assert.Equal(
            ExpectedLocation,
            jobSummary.Location);
    }

    [Fact]
    public void Extract_WhenTitleElementDoesNotExist_ReturnsMissingTitleFailure()
    {
        // Arrange
        var jobCardElement =
            Substitute.For<IWebElement>();

        jobCardElement
            .FindElement(SeekSelectors.Card.Title)
            .Returns(_ => throw new NoSuchElementException());

        var logger =
            Substitute.For<ILogger<SeekJobCardExtractor>>();

        var extractor =
            new SeekJobCardExtractor(logger);

        // Act
        var extractionResult =
            extractor.Extract(jobCardElement);

        // Assert
        Assert.False(extractionResult.IsSuccessful);
        Assert.Null(extractionResult.Value);

        var extractionIssue =
            Assert.Single(extractionResult.Issues);

        Assert.Equal(
            "Title",
            extractionIssue.Field);

        Assert.Equal(
            "Missing job title",
            extractionIssue.Message);

        Assert.Equal(
            ExtractionIssueSeverity.Error,
            extractionIssue.Severity);
    }
}
