using JobFlowAutomation.Domain;
using JobFlowAutomation.Infrastructure.Seek;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenQA.Selenium;

namespace JobFlowAutomation.UnitTests.Infrastructure.Seek;

public sealed class SeekJobDetailPageExtractorTests
{
    [Fact]
    public void Extract_WhenPageContainsValidData_ReturnsSuccessfulJobDetail()
    {
        // Arrange
        const string ExpectedTitle = "Senior Software Engineer";
        const string ExpectedDescription =
            "Develop and maintain modern .NET applications.";
        const string ExpectedAdvertiserName = "Example Technology";
        const string ExpectedLocation = "Melbourne VIC";
        const string ExpectedClassifications =
            "Information and Communication Technology";
        const string ExpectedWorkType = "Full time";
        const string ExpectedSalary = "$120,000 - $140,000";

        var webDriver = Substitute.For<IWebDriver>();

        var titleElement = Substitute.For<IWebElement>();
        var descriptionElement = Substitute.For<IWebElement>();
        var advertiserElement = Substitute.For<IWebElement>();
        var locationElement = Substitute.For<IWebElement>();
        var classificationsElement = Substitute.For<IWebElement>();
        var workTypeElement = Substitute.For<IWebElement>();
        var salaryElement = Substitute.For<IWebElement>();
        var quickApplyElement = Substitute.For<IWebElement>();

        titleElement.Text.Returns(
            $"  {ExpectedTitle}  ");

        descriptionElement.Text.Returns(
            $"  {ExpectedDescription}  ");

        advertiserElement.Text.Returns(
            $"  {ExpectedAdvertiserName}  ");

        locationElement.Text.Returns(
            $"  {ExpectedLocation}  ");

        classificationsElement.Text.Returns(
            $"  {ExpectedClassifications}  ");

        workTypeElement.Text.Returns(
            $"  {ExpectedWorkType}  ");

        salaryElement.Text.Returns(
            $"  {ExpectedSalary}  ");

        quickApplyElement.Text.Returns(
            "  Quick apply  ");

        webDriver
            .FindElement(SeekSelectors.Detail.Title)
            .Returns(titleElement);

        webDriver
            .FindElement(SeekSelectors.Detail.Description)
            .Returns(descriptionElement);

        webDriver
            .FindElement(SeekSelectors.Card.Company)
            .Returns(advertiserElement);

        webDriver
            .FindElement(SeekSelectors.Card.Location)
            .Returns(locationElement);

        webDriver
            .FindElement(SeekSelectors.Detail.Classifications)
            .Returns(classificationsElement);

        webDriver
            .FindElement(SeekSelectors.Detail.WorkType)
            .Returns(workTypeElement);

        webDriver
            .FindElement(SeekSelectors.Detail.Salary)
            .Returns(salaryElement);

        webDriver
            .FindElement(SeekSelectors.Detail.IsQuickApply)
            .Returns(quickApplyElement);

        var logger =
            Substitute.For<ILogger<SeekJobDetailPageExtractor>>();

        var extractor =
            new SeekJobDetailPageExtractor(
                webDriver,
                logger);

        // Act
        var extractionResult =
            extractor.Extract();

        // Assert
        Assert.True(extractionResult.IsSuccessful);
        Assert.Empty(extractionResult.Issues);

        var jobDetail =
            Assert.IsType<JobDetail>(extractionResult.Value);

        Assert.Equal(
            ExpectedTitle,
            jobDetail.Title);

        Assert.Equal(
            ExpectedDescription,
            jobDetail.Description);

        Assert.Equal(
            ExpectedAdvertiserName,
            jobDetail.AdvertiserName);

        Assert.Equal(
            ExpectedLocation,
            jobDetail.Location);

        Assert.Equal(
            ExpectedClassifications,
            jobDetail.Classifications);

        Assert.Equal(
            ExpectedWorkType,
            jobDetail.WorkType);

        Assert.Equal(
            ExpectedSalary,
            jobDetail.Salary);

        Assert.True(jobDetail.IsQuickApply);
    }

    [Fact]
    public void Extract_WhenTitleIsEmpty_ReturnsMissingTitleFailure()
    {
        // Arrange
        var webDriver = Substitute.For<IWebDriver>();

        var titleElement = Substitute.For<IWebElement>();
        var descriptionElement = Substitute.For<IWebElement>();

        titleElement.Text.Returns("   ");

        descriptionElement.Text.Returns(
            "A valid job description.");

        webDriver
            .FindElement(SeekSelectors.Detail.Title)
            .Returns(titleElement);

        webDriver
            .FindElement(SeekSelectors.Detail.Description)
            .Returns(descriptionElement);

        var logger =
            Substitute.For<ILogger<SeekJobDetailPageExtractor>>();

        var extractor =
            new SeekJobDetailPageExtractor(
                webDriver,
                logger);

        // Act
        var extractionResult =
            extractor.Extract();

        // Assert
        Assert.False(extractionResult.IsSuccessful);
        Assert.Null(extractionResult.Value);

        var extractionIssue =
            Assert.Single(extractionResult.Issues);

        Assert.Equal(
            "Title",
            extractionIssue.Field);

        Assert.Equal(
            "Missing title",
            extractionIssue.Message);

        Assert.Equal(
            ExtractionIssueSeverity.Error,
            extractionIssue.Severity);
    }
}
