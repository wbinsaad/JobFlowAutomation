namespace JobFlowAutomation.Application.Models;

/// <summary>
/// Represents the outcome of validating a configured CV file.
/// </summary>
public sealed record CvFileValidationResult
{
    private CvFileValidationResult(
        bool isValid,
        string? normalizedFilePath,
        CvFileValidationErrorCode? errorCode,
        string? errorMessage)
    {
        IsValid = isValid;
        NormalizedFilePath = normalizedFilePath;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsValid
    {
        get;
    }

    public string? NormalizedFilePath
    {
        get;
    }

    public CvFileValidationErrorCode? ErrorCode
    {
        get;
    }

    public string? ErrorMessage
    {
        get;
    }

    public static CvFileValidationResult Success(
        string normalizedFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            normalizedFilePath);

        return new CvFileValidationResult(
            isValid: true,
            normalizedFilePath,
            errorCode: null,
            errorMessage: null);
    }

    public static CvFileValidationResult Failure(
        CvFileValidationErrorCode errorCode,
        string errorMessage,
        string? normalizedFilePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            errorMessage);

        return new CvFileValidationResult(
            isValid: false,
            normalizedFilePath,
            errorCode,
            errorMessage);
    }
}
