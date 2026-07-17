using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Application.Models;
using JobFlowAutomation.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobFlowAutomation.Application.Services;

public sealed partial class CvSelector : ICvSelector
{
    private const int TitleKeywordScore = 100;
    private const int DescriptionKeywordScore = 10;

    private readonly bool _enabled;
    private readonly bool _requiresManualApproval;
    private readonly IReadOnlyList<CvProfile> _profiles;
    private readonly ILogger<CvSelector> _logger;

    public CvSelector(
        IOptions<CvSelectionOptions> options,
        ILogger<CvSelector> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        CvSelectionOptions configuredOptions = options.Value;

        _enabled = configuredOptions.Enabled;
        _requiresManualApproval =
            configuredOptions.RequireManualApproval;

        // Copy mutable configuration objects into a private,
        // read-only snapshot for the lifetime of this singleton.
        _profiles = CreateProfiles(
            configuredOptions.Profiles);
    }

    public CvSelectionResult? Select(
        string jobTitle,
        string? jobDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            jobTitle);

        string normalizedTitle = Normalize(jobTitle);
        string normalizedDescription =
            Normalize(jobDescription);

        LogSelectionStarted(
            normalizedTitle,
            _profiles.Count);

        if (!_enabled)
        {
            LogSelectionDisabled();
            return null;
        }

        List<CvCandidate> candidates = [];

        foreach (CvProfile profile in _profiles)
        {
            CvCandidate candidate = EvaluateProfile(
                profile,
                normalizedTitle,
                normalizedDescription);

            if (candidate.MatchedTitleKeywords.Count > 0)
            {
                candidates.Add(candidate);
            }
        }

        if (candidates.Count == 0)
        {
            LogNoMatchingProfile(normalizedTitle);
            return null;
        }

        CvCandidate selectedCandidate = candidates
            .OrderByDescending(candidate =>
                candidate.Score)
            .ThenByDescending(candidate =>
                candidate.Profile.Priority)
            .ThenBy(
                candidate => candidate.Profile.Name,
                StringComparer.Ordinal)
            .First();

        LogProfileSelected(
            selectedCandidate.Profile.Name,
            selectedCandidate.Score,
            selectedCandidate.MatchedTitleKeywords.Count,
            selectedCandidate
                .MatchedDescriptionKeywords
                .Count);

        return new CvSelectionResult(
            selectedCandidate.Profile.Name,
            selectedCandidate.Profile.FilePath,
            selectedCandidate.Score,
            selectedCandidate.MatchedTitleKeywords,
            selectedCandidate.MatchedDescriptionKeywords,
            _requiresManualApproval);
    }

    private static CvCandidate EvaluateProfile(
        CvProfile profile,
        string normalizedTitle,
        string normalizedDescription)
    {
        IReadOnlyList<string> matchedTitleKeywords =
            FindMatches(
                normalizedTitle,
                profile.TitleKeywords);

        IReadOnlyList<string> matchedDescriptionKeywords =
            FindMatches(
                normalizedDescription,
                profile.DescriptionKeywords);

        int score =
            (matchedTitleKeywords.Count * TitleKeywordScore)
            + (matchedDescriptionKeywords.Count
                * DescriptionKeywordScore);

        return new CvCandidate(
            profile,
            score,
            matchedTitleKeywords,
            matchedDescriptionKeywords);
    }

    private static IReadOnlyList<string> FindMatches(
        string text,
        IReadOnlyList<string> keywords)
    {
        if (string.IsNullOrEmpty(text)
            || keywords.Count == 0)
        {
            return Array.Empty<string>();
        }

        string[] matches = keywords
            .Where(keyword =>
                text.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return Array.AsReadOnly(matches);
    }

    private static IReadOnlyList<CvProfile> CreateProfiles(
        IReadOnlyList<CvProfileOptions>? profiles)
    {
        if (profiles is null || profiles.Count == 0)
        {
            return Array.Empty<CvProfile>();
        }

        CvProfile[] snapshot = profiles
            .Select(CreateProfile)
            .ToArray();

        return Array.AsReadOnly(snapshot);
    }

    private static CvProfile CreateProfile(
        CvProfileOptions profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new CvProfile(
            Name: profile.Name.Trim(),
            FilePath: profile.FilePath.Trim(),
            Priority: profile.Priority,
            TitleKeywords: CreateKeywordSnapshot(
                profile.TitleKeywords),
            DescriptionKeywords: CreateKeywordSnapshot(
                profile.DescriptionKeywords));
    }

    private static IReadOnlyList<string> CreateKeywordSnapshot(
        IReadOnlyList<string>? keywords)
    {
        if (keywords is null || keywords.Count == 0)
        {
            return Array.Empty<string>();
        }

        string[] snapshot = keywords
            .Where(keyword =>
                !string.IsNullOrWhiteSpace(keyword))
            .Select(Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Array.AsReadOnly(snapshot);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string[] segments = value.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries);

        return string.Join(
            " ",
            segments);
    }

    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Debug,
        Message =
            "Evaluating {ProfileCount} CV profiles for job title {JobTitle}.")]
    private partial void LogSelectionStarted(
        string jobTitle,
        int profileCount);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message =
            "CV selection is disabled. No profile will be selected.")]
    private partial void LogSelectionDisabled();

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Debug,
        Message =
            "No CV profile contains a title keyword matching job title {JobTitle}.")]
    private partial void LogNoMatchingProfile(
        string jobTitle);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message =
            "Selected CV profile {ProfileName} with score {Score}. "
            + "Title matches: {TitleMatchCount}; "
            + "description matches: {DescriptionMatchCount}.")]
    private partial void LogProfileSelected(
        string profileName,
        int score,
        int titleMatchCount,
        int descriptionMatchCount);

    private sealed record CvProfile(
        string Name,
        string FilePath,
        int Priority,
        IReadOnlyList<string> TitleKeywords,
        IReadOnlyList<string> DescriptionKeywords);

    private sealed record CvCandidate(
        CvProfile Profile,
        int Score,
        IReadOnlyList<string> MatchedTitleKeywords,
        IReadOnlyList<string> MatchedDescriptionKeywords);
}
