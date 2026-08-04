using JobFlowAutomation.Application.Models;
using JobFlowAutomation.Infrastructure.FileSystem;

using Microsoft.Extensions.Logging.Abstractions;

namespace JobFlowAutomation.UnitTests.Infrastructure.FileSystem;

public sealed class CvFileValidatorTests
    : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly CvFileValidator _validator;

    public CvFileValidatorTests()
    {
        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "JobFlowAutomation.UnitTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            _temporaryDirectory);

        _validator = new CvFileValidator(
            NullLogger<CvFileValidator>.Instance);
    }

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".doc")]
    [InlineData(".docx")]
    [InlineData(".PDF")]
    [InlineData(".DOCX")]
    public void Validate_WhenSupportedFileExists_ReturnsSuccess(
        string extension)
    {
        // Arrange
        string filePath = CreateFile(
            $"Resume{extension}");

        // Act
        CvFileValidationResult result =
            _validator.Validate(filePath);

        // Assert
        Assert.True(result.IsValid);

        Assert.Equal(
            Path.GetFullPath(filePath),
            result.NormalizedFilePath);

        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenPathIsMissing_ReturnsMissingPath(
        string? filePath)
    {
        // Act
        CvFileValidationResult result =
            _validator.Validate(filePath);

        // Assert
        Assert.False(result.IsValid);

        Assert.Equal(
            CvFileValidationErrorCode.MissingPath,
            result.ErrorCode);

        Assert.Null(result.NormalizedFilePath);
    }

    [Fact]
    public void Validate_WhenPathIsRelative_ReturnsInvalidPath()
    {
        // Act
        CvFileValidationResult result =
            _validator.Validate(
                "Resume.pdf");

        // Assert
        Assert.False(result.IsValid);

        Assert.Equal(
            CvFileValidationErrorCode.InvalidPath,
            result.ErrorCode);
    }

    [Fact]
    public void Validate_WhenPathContainsInvalidCharacter_ReturnsInvalidPath()
    {
        // Arrange
        string invalidPath =
            _temporaryDirectory
            + Path.DirectorySeparatorChar
            + "Invalid\0Resume.pdf";

        // Act
        CvFileValidationResult result =
            _validator.Validate(invalidPath);

        // Assert
        Assert.False(result.IsValid);

        Assert.Equal(
            CvFileValidationErrorCode.InvalidPath,
            result.ErrorCode);
    }

    [Fact]
    public void Validate_WhenFileDoesNotExist_ReturnsFileNotFound()
    {
        // Arrange
        string filePath = Path.Combine(
            _temporaryDirectory,
            "Missing.pdf");

        // Act
        CvFileValidationResult result =
            _validator.Validate(filePath);

        // Assert
        Assert.False(result.IsValid);

        Assert.Equal(
            CvFileValidationErrorCode.FileNotFound,
            result.ErrorCode);

        Assert.Equal(
            Path.GetFullPath(filePath),
            result.NormalizedFilePath);
    }

    [Fact]
    public void Validate_WhenPathPointsToDirectory_ReturnsPathIsDirectory()
    {
        // Act
        CvFileValidationResult result =
            _validator.Validate(
                _temporaryDirectory);

        // Assert
        Assert.False(result.IsValid);

        Assert.Equal(
            CvFileValidationErrorCode.PathIsDirectory,
            result.ErrorCode);

        Assert.Equal(
            Path.GetFullPath(_temporaryDirectory),
            result.NormalizedFilePath);
    }

    [Fact]
    public void Validate_WhenExtensionIsUnsupported_ReturnsUnsupportedExtension()
    {
        // Arrange
        string filePath = CreateFile(
            "Resume.txt");

        // Act
        CvFileValidationResult result =
            _validator.Validate(filePath);

        // Assert
        Assert.False(result.IsValid);

        Assert.Equal(
            CvFileValidationErrorCode.UnsupportedExtension,
            result.ErrorCode);

        Assert.Contains(
            ".pdf",
            result.ErrorMessage,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_WhenAbsolutePathHasOuterWhitespace_NormalizesPath()
    {
        // Arrange
        string filePath = CreateFile(
            "Resume.pdf");

        // Act
        CvFileValidationResult result =
            _validator.Validate(
                $"   {filePath}   ");

        // Assert
        Assert.True(result.IsValid);

        Assert.Equal(
            Path.GetFullPath(filePath),
            result.NormalizedFilePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(
            _temporaryDirectory))
        {
            Directory.Delete(
                _temporaryDirectory,
                recursive: true);
        }
    }

    private string CreateFile(
        string fileName)
    {
        string filePath = Path.Combine(
            _temporaryDirectory,
            fileName);

        File.WriteAllText(
            filePath,
            "Test CV file.");

        return filePath;
    }
}
