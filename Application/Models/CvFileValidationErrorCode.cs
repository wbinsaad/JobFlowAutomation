namespace JobFlowAutomation.Application.Models;

/// <summary>
/// Identifies why a configured CV file failed validation.
/// </summary>
public enum CvFileValidationErrorCode
{
    MissingPath,
    InvalidPath,
    FileNotFound,
    PathIsDirectory,
    UnsupportedExtension,
    AccessDenied,
    IoError
}
