using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Application.Models;
using JobFlowAutomation.Application.Services;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

namespace JobFlowAutomation.UnitTests.Application.Services;

public sealed class ApplicationPreparationServiceTests
{
    private const string CvFilePath =
        @"C:\TestData\DotNetDeveloper.pdf";

    [Fact]
    public void Prepare_WhenCvMatchesAndFileIsValid_ReturnsAwaitingReview()
    {
        // Arrange
        ApplicationPreparationRequest request =
            CreateValidRequest();

        CvSelectionResult cvSelection =
            CreateCvSelection();

        CvFileValidationResult fileValidation =
            CvFileValidationResult.Success(
                CvFilePath);

        ICvSelector cvSelector =
            Substitute.For<ICvSelector>();

        ICvFileValidator cvFileValidator =
            Substitute.For<ICvFileValidator>();

        cvSelector
            .Select(
                request.JobTitle,
                request.JobDescription)
            .Returns(cvSelection);

        cvFileValidator
            .Validate(CvFilePath)
            .Returns(fileValidation);

        ApplicationPreparationService service =
            CreateService(
                cvSelector,
                cvFileValidator);

        // Act
        ApplicationPreparationResult result =
            service.Prepare(request);

        // Assert
        Assert.Equal(
            ApplicationPreparationStatus.AwaitingReview,
            result.Status);

        Assert.Same(
            cvSelection,
            result.CvSelection);

        Assert.Same(
            fileValidation,
            result.CvFileValidation);

        Assert.True(
            result.RequiresManualReview);

        cvSelector.Received(1).Select(
            request.JobTitle,
            request.JobDescription);

        cvFileValidator.Received(1).Validate(
            CvFilePath);
    }

    [Fact]
    public void Prepare_WhenNoCvMatches_ReturnsNoMatchingCv()
    {
        // Arrange
        ApplicationPreparationRequest request =
            CreateValidRequest();

        ICvSelector cvSelector =
            Substitute.For<ICvSelector>();

        ICvFileValidator cvFileValidator =
            Substitute.For<ICvFileValidator>();

        cvSelector
            .Select(
                request.JobTitle,
                request.JobDescription)
            .Returns(
                (CvSelectionResult?)null);

        ApplicationPreparationService service =
            CreateService(
                cvSelector,
                cvFileValidator);

        // Act
        ApplicationPreparationResult result =
            service.Prepare(request);

        // Assert
        Assert.Equal(
            ApplicationPreparationStatus.NoMatchingCv,
            result.Status);

        Assert.Null(
            result.CvSelection);

        Assert.Null(
            result.CvFileValidation);

        Assert.False(
            result.RequiresManualReview);

        cvFileValidator
            .DidNotReceive()
            .Validate(
                Arg.Any<string?>());
    }

    [Fact]
    public void Prepare_WhenCvFileIsMissing_ReturnsInvalidCvFile()
    {
        // Arrange
        ApplicationPreparationRequest request =
            CreateValidRequest();

        CvSelectionResult cvSelection =
            CreateCvSelection();

        CvFileValidationResult fileValidation =
            CvFileValidationResult.Failure(
                CvFileValidationErrorCode.FileNotFound,
                "The configured CV file could not be found.",
                CvFilePath);

        ICvSelector cvSelector =
            Substitute.For<ICvSelector>();

        ICvFileValidator cvFileValidator =
            Substitute.For<ICvFileValidator>();

        cvSelector
            .Select(
                request.JobTitle,
                request.JobDescription)
            .Returns(cvSelection);

        cvFileValidator
            .Validate(CvFilePath)
            .Returns(fileValidation);

        ApplicationPreparationService service =
            CreateService(
                cvSelector,
                cvFileValidator);

        // Act
        ApplicationPreparationResult result =
            service.Prepare(request);

        // Assert
        Assert.Equal(
            ApplicationPreparationStatus.InvalidCvFile,
            result.Status);

        Assert.Same(
            cvSelection,
            result.CvSelection);

        Assert.Same(
            fileValidation,
            result.CvFileValidation);

        Assert.Equal(
            CvFileValidationErrorCode.FileNotFound,
            result.CvFileValidation.ErrorCode);

        Assert.False(
            result.RequiresManualReview);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Prepare_WhenJobTitleIsInvalid_ReturnsInvalidJobData(
        string? jobTitle)
    {
        // Arrange
        ApplicationPreparationRequest request =
            CreateValidRequest() with
            {
                JobTitle = jobTitle!
            };

        ICvSelector cvSelector =
            Substitute.For<ICvSelector>();

        ICvFileValidator cvFileValidator =
            Substitute.For<ICvFileValidator>();

        ApplicationPreparationService service =
            CreateService(
                cvSelector,
                cvFileValidator);

        // Act
        ApplicationPreparationResult result =
            service.Prepare(request);

        // Assert
        Assert.Equal(
            ApplicationPreparationStatus.InvalidJobData,
            result.Status);

        Assert.Contains(
            "Job title",
            result.Message,
            StringComparison.Ordinal);

        cvSelector
            .DidNotReceive()
            .Select(
                Arg.Any<string>(),
                Arg.Any<string?>());

        cvFileValidator
            .DidNotReceive()
            .Validate(
                Arg.Any<string?>());
    }

    [Fact]
    public void Prepare_WhenJobUrlIsRelative_ReturnsInvalidJobData()
    {
        // Arrange
        ApplicationPreparationRequest request =
            CreateValidRequest() with
            {
                JobUrl = new Uri(
                    "/jobs/12345",
                    UriKind.Relative)
            };

        ICvSelector cvSelector =
            Substitute.For<ICvSelector>();

        ICvFileValidator cvFileValidator =
            Substitute.For<ICvFileValidator>();

        ApplicationPreparationService service =
            CreateService(
                cvSelector,
                cvFileValidator);

        // Act
        ApplicationPreparationResult result =
            service.Prepare(request);

        // Assert
        Assert.Equal(
            ApplicationPreparationStatus.InvalidJobData,
            result.Status);

        Assert.Contains(
            "HTTP or HTTPS",
            result.Message,
            StringComparison.Ordinal);

        cvSelector
            .DidNotReceive()
            .Select(
                Arg.Any<string>(),
                Arg.Any<string?>());
    }

    [Fact]
    public void Prepare_WhenJobUrlUsesUnsupportedScheme_ReturnsInvalidJobData()
    {
        // Arrange
        ApplicationPreparationRequest request =
            CreateValidRequest() with
            {
                JobUrl = new Uri(
                    "ftp://example.test/jobs/12345")
            };

        ICvSelector cvSelector =
            Substitute.For<ICvSelector>();

        ICvFileValidator cvFileValidator =
            Substitute.For<ICvFileValidator>();

        ApplicationPreparationService service =
            CreateService(
                cvSelector,
                cvFileValidator);

        // Act
        ApplicationPreparationResult result =
            service.Prepare(request);

        // Assert
        Assert.Equal(
            ApplicationPreparationStatus.InvalidJobData,
            result.Status);

        cvSelector
            .DidNotReceive()
            .Select(
                Arg.Any<string>(),
                Arg.Any<string?>());
    }

    [Fact]
    public void Prepare_WhenSuccessful_PreservesSelectionExplanation()
    {
        // Arrange
        ApplicationPreparationRequest request =
            CreateValidRequest();

        CvSelectionResult cvSelection =
            CreateCvSelection();

        CvFileValidationResult fileValidation =
            CvFileValidationResult.Success(
                CvFilePath);

        ICvSelector cvSelector =
            Substitute.For<ICvSelector>();

        ICvFileValidator cvFileValidator =
            Substitute.For<ICvFileValidator>();

        cvSelector
            .Select(
                request.JobTitle,
                request.JobDescription)
            .Returns(cvSelection);

        cvFileValidator
            .Validate(CvFilePath)
            .Returns(fileValidation);

        ApplicationPreparationService service =
            CreateService(
                cvSelector,
                cvFileValidator);

        // Act
        ApplicationPreparationResult result =
            service.Prepare(request);

        // Assert
        Assert.NotNull(
            result.CvSelection);

        Assert.Equal(
            120,
            result.CvSelection.Score);

        Assert.Equal(
            [".net developer"],
            result.CvSelection.MatchedTitleKeywords);

        Assert.Equal(
            ["c#", "asp.net core"],
            result.CvSelection.MatchedDescriptionKeywords);
    }

    [Fact]
    public void Prepare_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        ICvSelector cvSelector =
            Substitute.For<ICvSelector>();

        ICvFileValidator cvFileValidator =
            Substitute.For<ICvFileValidator>();

        ApplicationPreparationService service =
            CreateService(
                cvSelector,
                cvFileValidator);

        // Act
        Action action = () =>
            service.Prepare(
                request: null!);

        // Assert
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                action);

        Assert.Equal(
            "request",
            exception.ParamName);
    }

    private static ApplicationPreparationService CreateService(
        ICvSelector cvSelector,
        ICvFileValidator cvFileValidator)
    {
        return new ApplicationPreparationService(
            cvSelector,
            cvFileValidator,
            NullLogger<ApplicationPreparationService>
                .Instance);
    }

    private static ApplicationPreparationRequest
        CreateValidRequest()
    {
        return new ApplicationPreparationRequest(
            JobTitle: ".NET Developer",
            JobDescription:
                "Build services using C# and ASP.NET Core.",
            Company: "Example Technology",
            JobUrl: new Uri(
                "https://example.test/jobs/12345"));
    }

    private static CvSelectionResult CreateCvSelection()
    {
        return new CvSelectionResult(
            ProfileName: "DotNetDeveloper",
            FilePath: CvFilePath,
            Score: 120,
            MatchedTitleKeywords:
            [
                ".net developer"
            ],
            MatchedDescriptionKeywords:
            [
                "c#",
                "asp.net core"
            ],
            RequiresManualApproval: true);
    }
}
