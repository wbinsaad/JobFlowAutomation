using JobFlowAutomation.Application.Models;

namespace JobFlowAutomation.Application.Abstractions;

/// <summary>
/// Validates a configured CV file before application preparation.
/// </summary>
public interface ICvFileValidator
{
    /// <summary>
    /// Validates that a CV path identifies an accessible,
    /// supported file.
    /// </summary>
    /// <param name="filePath">
    /// The configured CV file path.
    /// </param>
    /// <returns>
    /// A result describing whether the file is valid.
    /// </returns>
    CvFileValidationResult Validate(
        string? filePath);
}
