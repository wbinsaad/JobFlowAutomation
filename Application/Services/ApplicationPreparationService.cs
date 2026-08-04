using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Application.Models;

using Microsoft.Extensions.Logging;

namespace JobFlowAutomation.Application.Services;

public sealed partial class ApplicationPreparationService
    : IApplicationPreparationService
{
    private readonly ICvSelector _cvSelector;
    private readonly ICvFileValidator _cvFileValidator;
    private readonly ILogger<ApplicationPreparationService>
        _logger;

    public ApplicationPreparationService(
        ICvSelector cvSelector,
        ICvFileValidator cvFileValidator,
        ILogger<ApplicationPreparationService> logger)
    {
        ArgumentNullException.ThrowIfNull(
            cvSelector);

        ArgumentNullException.ThrowIfNull(
            cvFileValidator);

        ArgumentNullException.ThrowIfNull(
            logger);

        _cvSelector = cvSelector;
        _cvFileValidator = cvFileValidator;
        _logger = logger;
    }

    public ApplicationPreparationResult Prepare(
        ApplicationPreparationRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        LogPreparationStarted(
            request.JobTitle);

        string? validationFailure =
            ValidateRequest(request);

        if (validationFailure is not null)
        {
            LogInvalidJobData(
                validationFailure);

            return ApplicationPreparationResult
                .InvalidJobData(
                    validationFailure);
        }

        CvSelectionResult? cvSelection =
            _cvSelector.Select(
                request.JobTitle,
                request.JobDescription);

        if (cvSelection is null)
        {
            LogNoMatchingCv(
                request.JobTitle);

            return ApplicationPreparationResult
                .NoMatchingCv();
        }

        LogCvSelected(
            cvSelection.ProfileName,
            cvSelection.Score);

        CvFileValidationResult cvFileValidation =
            _cvFileValidator.Validate(
                cvSelection.FilePath);

        if (!cvFileValidation.IsValid)
        {
            LogInvalidCvFile(
                cvSelection.ProfileName,
                cvFileValidation.ErrorCode);

            return ApplicationPreparationResult
                .InvalidCvFile(
                    cvSelection,
                    cvFileValidation);
        }

        LogAwaitingReview(
            cvSelection.ProfileName);

        return ApplicationPreparationResult
            .AwaitingReview(
                cvSelection,
                cvFileValidation);
    }

    private static string? ValidateRequest(
        ApplicationPreparationRequest request)
    {
        if (string.IsNullOrWhiteSpace(
            request.JobTitle))
        {
            return "Job title is required.";
        }

        if (!IsSupportedJobUrl(
            request.JobUrl))
        {
            return
                "Job URL must be an absolute HTTP or HTTPS URL.";
        }

        return null;
    }

    private static bool IsSupportedJobUrl(
        Uri? jobUrl)
    {
        if (jobUrl is null
            || !jobUrl.IsAbsoluteUri
            || string.IsNullOrWhiteSpace(jobUrl.Host))
        {
            return false;
        }

        return jobUrl.Scheme.Equals(
                   Uri.UriSchemeHttp,
                   StringComparison.OrdinalIgnoreCase)
               || jobUrl.Scheme.Equals(
                   Uri.UriSchemeHttps,
                   StringComparison.OrdinalIgnoreCase);
    }

    [LoggerMessage(
        EventId = 2200,
        Level = LogLevel.Debug,
        Message =
            "Preparing an application for job title {JobTitle}.")]
    private partial void LogPreparationStarted(
        string? jobTitle);

    [LoggerMessage(
        EventId = 2201,
        Level = LogLevel.Warning,
        Message =
            "Application preparation rejected invalid job data. "
            + "Reason: {Reason}")]
    private partial void LogInvalidJobData(
        string reason);

    [LoggerMessage(
        EventId = 2202,
        Level = LogLevel.Information,
        Message =
            "No configured CV matches job title {JobTitle}.")]
    private partial void LogNoMatchingCv(
        string jobTitle);

    [LoggerMessage(
        EventId = 2203,
        Level = LogLevel.Debug,
        Message =
            "Selected CV profile {ProfileName} with score {Score} "
            + "during application preparation.")]
    private partial void LogCvSelected(
        string profileName,
        int score);

    [LoggerMessage(
        EventId = 2204,
        Level = LogLevel.Warning,
        Message =
            "Selected CV profile {ProfileName} failed file validation "
            + "with error code {ErrorCode}.")]
    private partial void LogInvalidCvFile(
        string profileName,
        CvFileValidationErrorCode? errorCode);

    [LoggerMessage(
        EventId = 2205,
        Level = LogLevel.Information,
        Message =
            "Application using CV profile {ProfileName} "
            + "is awaiting manual review.")]
    private partial void LogAwaitingReview(
        string profileName);
}
