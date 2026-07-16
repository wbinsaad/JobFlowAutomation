using JobFlowAutomation.Configuration;

using Microsoft.Extensions.Options;

namespace JobFlowAutomation.UnitTests.Configuration;

public sealed class CvSelectionOptionsValidatorTests
{
    private readonly CvSelectionOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenFeatureIsDisabledAndProfilesAreEmpty_Succeeds()
    {
        var options = new CvSelectionOptions
        {
            Enabled = false,
            Profiles = []
        };

        var result = _validator.Validate(
            name: null,
            options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WhenConfigurationIsValid_Succeeds()
    {
        var options = CreateValidOptions();

        var result = _validator.Validate(
            name: null,
            options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WhenEnabledWithoutProfiles_Fails()
    {
        var options = CreateValidOptions();
        options.Profiles = [];

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "must contain at least one CV profile");
    }

    [Fact]
    public void Validate_WhenManualApprovalIsDisabled_Fails()
    {
        var options = CreateValidOptions();
        options.RequireManualApproval = false;

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "RequireManualApproval");
    }

    [Fact]
    public void Validate_WhenProfileNameIsEmpty_Fails()
    {
        var options = CreateValidOptions();
        options.Profiles[0].Name = " ";

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "Name must not be empty");
    }

    [Fact]
    public void Validate_WhenProfileNamesAreDuplicatedIgnoringCase_Fails()
    {
        var options = CreateValidOptions();

        options.Profiles.Add(
            CreateValidProfile(
                "dotnetdeveloper"));

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "duplicate profile name");
    }

    [Fact]
    public void Validate_WhenFilePathIsEmpty_Fails()
    {
        var options = CreateValidOptions();
        options.Profiles[0].FilePath = string.Empty;

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "FilePath must not be empty");
    }

    [Fact]
    public void Validate_WhenPriorityIsNegative_Fails()
    {
        var options = CreateValidOptions();
        options.Profiles[0].Priority = -1;

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "Priority must not be negative");
    }

    [Fact]
    public void Validate_WhenTitleKeywordsAreEmpty_Fails()
    {
        var options = CreateValidOptions();
        options.Profiles[0].TitleKeywords = [];

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "TitleKeywords must contain at least one keyword");
    }

    [Fact]
    public void Validate_WhenKeywordContainsOnlyWhitespace_Fails()
    {
        var options = CreateValidOptions();
        options.Profiles[0].TitleKeywords = ["   "];

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "TitleKeywords:0 must not be empty");
    }

    [Fact]
    public void Validate_WhenKeywordsAreDuplicatedAfterNormalization_Fails()
    {
        var options = CreateValidOptions();

        options.Profiles[0].TitleKeywords =
        [
            ".net developer",
            "  .NET DEVELOPER  "
        ];

        var result = _validator.Validate(
            name: null,
            options);

        AssertFailureContains(
            result,
            "duplicate keyword");
    }

    private static CvSelectionOptions CreateValidOptions()
    {
        return new CvSelectionOptions
        {
            Enabled = true,
            RequireManualApproval = true,
            Profiles =
            [
                CreateValidProfile(
                    "DotNetDeveloper")
            ]
        };
    }

    private static CvProfileOptions CreateValidProfile(
        string name)
    {
        return new CvProfileOptions
        {
            Name = name,
            FilePath =
                @"C:\TestData\DotNetDeveloper.pdf",
            Priority = 100,
            TitleKeywords =
            [
                ".net developer"
            ],
            DescriptionKeywords =
            [
                "c#",
                "asp.net core"
            ]
        };
    }

    private static void AssertFailureContains(
        ValidateOptionsResult result,
        string expectedText)
    {
        Assert.False(result.Succeeded);

        Assert.Contains(
            result.Failures,
            failure => failure.Contains(
                expectedText,
                StringComparison.Ordinal));
    }
}
