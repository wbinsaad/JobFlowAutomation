using JobFlowAutomation.Application.Services;
using JobFlowAutomation.Configuration;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JobFlowAutomation.UnitTests.Application.Services;

public sealed class CvSelectorTests
{
    [Fact]
    public void Select_WhenDotNetRoleMatches_ReturnsDotNetProfile()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "DotNetDeveloper",
                filePath: @"C:\TestData\DotNetDeveloper.pdf",
                priority: 100,
                titleKeywords:
                [
                    ".net developer"
                ],
                descriptionKeywords:
                [
                    "c#",
                    "asp.net core"
                ]));

        var result = selector.Select(
            " Senior .NET Developer ",
            "Build services using C# and ASP.NET Core.");

        Assert.NotNull(result);
        Assert.Equal(
            "DotNetDeveloper",
            result.ProfileName);
        Assert.Equal(
            @"C:\TestData\DotNetDeveloper.pdf",
            result.FilePath);
        Assert.Equal(
            120,
            result.Score);
        Assert.Equal(
            [".net developer"],
            result.MatchedTitleKeywords);
        Assert.Equal(
            ["c#", "asp.net core"],
            result.MatchedDescriptionKeywords);
        Assert.True(result.RequiresManualApproval);
    }

    [Fact]
    public void Select_WhenDescriptionIsNull_UsesTitleOnly()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "SoftwareEngineer",
                filePath: @"C:\TestData\SoftwareEngineer.pdf",
                priority: 100,
                titleKeywords:
                [
                    "software engineer"
                ]));

        var result = selector.Select(
            "Software Engineer",
            jobDescription: null);

        Assert.NotNull(result);
        Assert.Equal(
            100,
            result.Score);
        Assert.Empty(
            result.MatchedDescriptionKeywords);
    }

    [Fact]
    public void Select_WhenOnlyDescriptionMatches_ReturnsNull()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "PythonDeveloper",
                filePath: @"C:\TestData\PythonDeveloper.pdf",
                priority: 100,
                titleKeywords:
                [
                    "python developer"
                ],
                descriptionKeywords:
                [
                    "python"
                ]));

        var result = selector.Select(
            "IT Support Officer",
            "Some internal tools use Python.");

        Assert.Null(result);
    }

    [Fact]
    public void Select_WhenNoProfileMatches_ReturnsNull()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "DotNetDeveloper",
                filePath: @"C:\TestData\DotNetDeveloper.pdf",
                priority: 100,
                titleKeywords:
                [
                    ".net developer"
                ]));

        var result = selector.Select(
            "Graphic Designer",
            "Design marketing artwork.");

        Assert.Null(result);
    }

    [Fact]
    public void Select_WhenMultipleProfilesMatch_UsesHighestScore()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "SoftwareEngineer",
                filePath: @"C:\TestData\SoftwareEngineer.pdf",
                priority: 100,
                titleKeywords:
                [
                    "software engineer"
                ],
                descriptionKeywords:
                [
                    "api"
                ]),
            CreateProfile(
                name: "DotNetDeveloper",
                filePath: @"C:\TestData\DotNetDeveloper.pdf",
                priority: 100,
                titleKeywords:
                [
                    ".net software engineer"
                ],
                descriptionKeywords:
                [
                    "c#",
                    "asp.net core"
                ]));

        var result = selector.Select(
            ".NET Software Engineer",
            "Build API services using C# and ASP.NET Core.");

        Assert.NotNull(result);
        Assert.Equal(
            "DotNetDeveloper",
            result.ProfileName);
        Assert.Equal(
            120,
            result.Score);
    }

    [Fact]
    public void Select_WhenScoresAreEqual_UsesHigherPriority()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "GeneralSoftwareEngineer",
                filePath: @"C:\TestData\General.pdf",
                priority: 100,
                titleKeywords:
                [
                    "software engineer"
                ]),
            CreateProfile(
                name: "PreferredSoftwareEngineer",
                filePath: @"C:\TestData\Preferred.pdf",
                priority: 200,
                titleKeywords:
                [
                    "software engineer"
                ]));

        var result = selector.Select(
            "Software Engineer",
            string.Empty);

        Assert.NotNull(result);
        Assert.Equal(
            "PreferredSoftwareEngineer",
            result.ProfileName);
    }

    [Fact]
    public void Select_WhenScoresAndPrioritiesAreEqual_UsesProfileName()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "ZuluProfile",
                filePath: @"C:\TestData\Zulu.pdf",
                priority: 100,
                titleKeywords:
                [
                    "software engineer"
                ]),
            CreateProfile(
                name: "AlphaProfile",
                filePath: @"C:\TestData\Alpha.pdf",
                priority: 100,
                titleKeywords:
                [
                    "software engineer"
                ]));

        var result = selector.Select(
            "Software Engineer",
            string.Empty);

        Assert.NotNull(result);
        Assert.Equal(
            "AlphaProfile",
            result.ProfileName);
    }

    [Fact]
    public void Select_WhenTitleContainsRepeatedWhitespace_StillMatches()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "SoftwareEngineer",
                filePath: @"C:\TestData\SoftwareEngineer.pdf",
                priority: 100,
                titleKeywords:
                [
                    "software engineer"
                ]));

        var result = selector.Select(
            "Senior    SOFTWARE     Engineer",
            string.Empty);

        Assert.NotNull(result);
        Assert.Equal(
            "SoftwareEngineer",
            result.ProfileName);
    }

    [Fact]
    public void Select_WhenKeywordAppearsSeveralTimes_CountsItOnce()
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "DotNetDeveloper",
                filePath: @"C:\TestData\DotNetDeveloper.pdf",
                priority: 100,
                titleKeywords:
                [
                    ".net developer"
                ],
                descriptionKeywords:
                [
                    "c#"
                ]));

        var result = selector.Select(
            ".NET Developer",
            "C# services, C# APIs and more C# code.");

        Assert.NotNull(result);
        Assert.Equal(
            110,
            result.Score);
        Assert.Single(
            result.MatchedDescriptionKeywords);
    }

    [Fact]
    public void Select_WhenFeatureIsDisabled_ReturnsNull()
    {
        CvSelector selector = CreateSelector(
            enabled: false,
            CreateProfile(
                name: "SoftwareEngineer",
                filePath: @"C:\TestData\SoftwareEngineer.pdf",
                priority: 100,
                titleKeywords:
                [
                    "software engineer"
                ]));

        var result = selector.Select(
            "Software Engineer",
            string.Empty);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Select_WhenTitleIsInvalid_ThrowsArgumentException(
        string? jobTitle)
    {
        CvSelector selector = CreateSelector(
            CreateProfile(
                name: "SoftwareEngineer",
                filePath: @"C:\TestData\SoftwareEngineer.pdf",
                priority: 100,
                titleKeywords:
                [
                    "software engineer"
                ]));

        Assert.ThrowsAny<ArgumentException>(
            () => selector.Select(
                jobTitle!,
                string.Empty));
    }

    private static CvSelector CreateSelector(
        params CvProfileOptions[] profiles)
    {
        return CreateSelector(
            enabled: true,
            profiles);
    }

    private static CvSelector CreateSelector(
        bool enabled,
        params CvProfileOptions[] profiles)
    {
        var options = new CvSelectionOptions
        {
            Enabled = enabled,
            RequireManualApproval = true,
            Profiles = [.. profiles]
        };

        return new CvSelector(
            Options.Create(options),
            NullLogger<CvSelector>.Instance);
    }

    private static CvProfileOptions CreateProfile(
        string name,
        string filePath,
        int priority,
        IReadOnlyList<string> titleKeywords,
        IReadOnlyList<string>? descriptionKeywords = null)
    {
        return new CvProfileOptions
        {
            Name = name,
            FilePath = filePath,
            Priority = priority,
            TitleKeywords = [.. titleKeywords],
            DescriptionKeywords =
                descriptionKeywords is null
                    ? []
                    : [.. descriptionKeywords]
        };
    }
}
