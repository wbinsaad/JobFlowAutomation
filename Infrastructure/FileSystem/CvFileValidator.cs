using System.Security;

using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Application.Models;

using Microsoft.Extensions.Logging;

namespace JobFlowAutomation.Infrastructure.FileSystem;

public sealed partial class CvFileValidator
    : ICvFileValidator
{
    private const string SupportedExtensionsMessage =
        ".pdf, .doc, .docx";

    private static readonly string[]
        s_supportedExtensions =
        [
            ".pdf",
            ".doc",
            ".docx"
        ];

    private readonly ILogger<CvFileValidator> _logger;

    public CvFileValidator(
        ILogger<CvFileValidator> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public CvFileValidationResult Validate(
        string? filePath)
    {
        LogValidationStarted();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return CreateFailure(
                CvFileValidationErrorCode.MissingPath,
                "CV file path is required.");
        }

        string trimmedPath = filePath.Trim();

        if (!Path.IsPathFullyQualified(trimmedPath))
        {
            return CreateFailure(
                CvFileValidationErrorCode.InvalidPath,
                "CV file path must be an absolute path.");
        }

        string normalizedPath;

        try
        {
            normalizedPath = Path.GetFullPath(
                trimmedPath);
        }
        catch (ArgumentException exception)
        {
            return CreateInvalidPathFailure(
                exception);
        }
        catch (NotSupportedException exception)
        {
            return CreateInvalidPathFailure(
                exception);
        }
        catch (PathTooLongException exception)
        {
            return CreateInvalidPathFailure(
                exception);
        }
        catch (SecurityException exception)
        {
            return CreateAccessDeniedFailure(
                normalizedFilePath: null,
                exception);
        }

        FileAttributes attributes;

        try
        {
            attributes = File.GetAttributes(
                normalizedPath);
        }
        catch (UnauthorizedAccessException exception)
        {
            return CreateAccessDeniedFailure(
                normalizedPath,
                exception);
        }
        catch (SecurityException exception)
        {
            return CreateAccessDeniedFailure(
                normalizedPath,
                exception);
        }
        catch (FileNotFoundException exception)
        {
            return CreateFileNotFoundFailure(
                normalizedPath,
                exception);
        }
        catch (DirectoryNotFoundException exception)
        {
            return CreateFileNotFoundFailure(
                normalizedPath,
                exception);
        }
        catch (DriveNotFoundException exception)
        {
            return CreateFileNotFoundFailure(
                normalizedPath,
                exception);
        }
        catch (PathTooLongException exception)
        {
            return CreateInvalidPathFailure(
                exception,
                normalizedPath);
        }
        catch (NotSupportedException exception)
        {
            return CreateInvalidPathFailure(
                exception,
                normalizedPath);
        }
        catch (ArgumentException exception)
        {
            return CreateInvalidPathFailure(
                exception,
                normalizedPath);
        }
        catch (IOException exception)
        {
            return CreateFailure(
                CvFileValidationErrorCode.IoError,
                "The CV file could not be inspected because of an I/O error.",
                normalizedPath,
                exception);
        }

        if ((attributes & FileAttributes.Directory)
            == FileAttributes.Directory)
        {
            return CreateFailure(
                CvFileValidationErrorCode.PathIsDirectory,
                "The configured CV path points to a directory, not a file.",
                normalizedPath);
        }

        string extension = Path.GetExtension(
            normalizedPath);

        if (!IsSupportedExtension(extension))
        {
            return CreateFailure(
                CvFileValidationErrorCode.UnsupportedExtension,
                $"The CV file type is not supported. "
                + $"Supported extensions: {SupportedExtensionsMessage}.",
                normalizedPath);
        }

        LogValidationSucceeded(extension);

        return CvFileValidationResult.Success(
            normalizedPath);
    }

    private static bool IsSupportedExtension(
        string extension)
    {
        return s_supportedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    private CvFileValidationResult
        CreateInvalidPathFailure(
            Exception exception,
            string? normalizedFilePath = null)
    {
        return CreateFailure(
            CvFileValidationErrorCode.InvalidPath,
            "The configured CV file path is invalid.",
            normalizedFilePath,
            exception);
    }

    private CvFileValidationResult
        CreateFileNotFoundFailure(
            string normalizedFilePath,
            Exception exception)
    {
        return CreateFailure(
            CvFileValidationErrorCode.FileNotFound,
            "The configured CV file could not be found.",
            normalizedFilePath,
            exception);
    }

    private CvFileValidationResult
        CreateAccessDeniedFailure(
            string? normalizedFilePath,
            Exception exception)
    {
        return CreateFailure(
            CvFileValidationErrorCode.AccessDenied,
            "The application does not have permission "
            + "to access the configured CV file.",
            normalizedFilePath,
            exception);
    }

    private CvFileValidationResult CreateFailure(
        CvFileValidationErrorCode errorCode,
        string errorMessage,
        string? normalizedFilePath = null,
        Exception? exception = null)
    {
        string exceptionType =
            exception?.GetType().Name
            ?? "None";

        LogValidationFailed(
            errorCode,
            exceptionType);

        return CvFileValidationResult.Failure(
            errorCode,
            errorMessage,
            normalizedFilePath);
    }

    [LoggerMessage(
        EventId = 2100,
        Level = LogLevel.Debug,
        Message =
            "Validating the configured CV file.")]
    private partial void LogValidationStarted();

    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Debug,
        Message =
            "CV file validation succeeded for extension {Extension}.")]
    private partial void LogValidationSucceeded(
        string extension);

    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Warning,
        Message =
            "CV file validation failed with error code "
            + "{ErrorCode}. Exception type: {ExceptionType}.")]
    private partial void LogValidationFailed(
        CvFileValidationErrorCode errorCode,
        string exceptionType);
}
