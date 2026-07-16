using Microsoft.Extensions.Options;

namespace JobFlowAutomation.Configuration;

public sealed class CvSelectionOptionsValidator
    : IValidateOptions<CvSelectionOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        CvSelectionOptions options)
    {
        _ = name;

        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        List<string> failures = [];

        if (!options.RequireManualApproval)
        {
            failures.Add(
                $"{CvSelectionOptions.ConfigurationSectionName}:" +
                $"{nameof(CvSelectionOptions.RequireManualApproval)} " +
                "must be true while CV selection is enabled.");
        }

        if (options.Profiles is null ||
            options.Profiles.Count == 0)
        {
            failures.Add(
                $"{CvSelectionOptions.ConfigurationSectionName}:" +
                $"{nameof(CvSelectionOptions.Profiles)} " +
                "must contain at least one CV profile when CV selection is enabled.");
        }
        else
        {
            ValidateProfiles(
                options.Profiles,
                failures);
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateProfiles(
        IReadOnlyList<CvProfileOptions> profiles,
        ICollection<string> failures)
    {
        var profileNames = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < profiles.Count; index++)
        {
            CvProfileOptions? profile = profiles[index];

            var profilePath =
                $"{CvSelectionOptions.ConfigurationSectionName}:" +
                $"{nameof(CvSelectionOptions.Profiles)}:{index}";

            if (profile is null)
            {
                failures.Add(
                    $"{profilePath} must not be null.");

                continue;
            }

            ValidateProfileName(
                profile,
                profilePath,
                profileNames,
                failures);

            if (string.IsNullOrWhiteSpace(profile.FilePath))
            {
                failures.Add(
                    $"{profilePath}:" +
                    $"{nameof(CvProfileOptions.FilePath)} " +
                    "must not be empty.");
            }

            if (profile.Priority < 0)
            {
                failures.Add(
                    $"{profilePath}:" +
                    $"{nameof(CvProfileOptions.Priority)} " +
                    "must not be negative.");
            }

            ValidateKeywords(
                profile.TitleKeywords,
                $"{profilePath}:" +
                $"{nameof(CvProfileOptions.TitleKeywords)}",
                requireAtLeastOne: true,
                failures);

            ValidateKeywords(
                profile.DescriptionKeywords,
                $"{profilePath}:" +
                $"{nameof(CvProfileOptions.DescriptionKeywords)}",
                requireAtLeastOne: false,
                failures);
        }
    }

    private static void ValidateProfileName(
        CvProfileOptions profile,
        string profilePath,
        ISet<string> profileNames,
        ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            failures.Add(
                $"{profilePath}:" +
                $"{nameof(CvProfileOptions.Name)} " +
                "must not be empty.");

            return;
        }

        var normalizedName = profile.Name.Trim();

        if (!profileNames.Add(normalizedName))
        {
            failures.Add(
                $"{CvSelectionOptions.ConfigurationSectionName}:" +
                $"{nameof(CvSelectionOptions.Profiles)} " +
                $"contains the duplicate profile name '{normalizedName}'.");
        }
    }

    private static void ValidateKeywords(
        IReadOnlyList<string>? keywords,
        string configurationPath,
        bool requireAtLeastOne,
        ICollection<string> failures)
    {
        if (keywords is null || keywords.Count == 0)
        {
            if (requireAtLeastOne)
            {
                failures.Add(
                    $"{configurationPath} must contain at least one keyword.");
            }

            return;
        }

        var normalizedKeywords = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < keywords.Count; index++)
        {
            var keyword = keywords[index];

            if (string.IsNullOrWhiteSpace(keyword))
            {
                failures.Add(
                    $"{configurationPath}:{index} must not be empty.");

                continue;
            }

            var normalizedKeyword = keyword.Trim();

            if (!normalizedKeywords.Add(normalizedKeyword))
            {
                failures.Add(
                    $"{configurationPath} contains the duplicate keyword " +
                    $"'{normalizedKeyword}'.");
            }
        }
    }
}
