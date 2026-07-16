using JobFlowAutomation.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JobFlowAutomation.UnitTests.Configuration;

public sealed class CvSelectionConfigurationTests
{
    [Fact]
    public void AddCvSelectionOptions_WhenConfigurationIsValid_BindsProfiles()
    {
        var values = new Dictionary<string, string?>
        {
            ["CvSelection:Enabled"] = "true",
            ["CvSelection:RequireManualApproval"] = "true",
            ["CvSelection:Profiles:0:Name"] =
                "DotNetDeveloper",
            ["CvSelection:Profiles:0:FilePath"] =
                @"C:\TestData\DotNetDeveloper.pdf",
            ["CvSelection:Profiles:0:Priority"] =
                "110",
            ["CvSelection:Profiles:0:TitleKeywords:0"] =
                ".net developer",
            ["CvSelection:Profiles:0:DescriptionKeywords:0"] =
                "c#"
        };

        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();

        var services = new ServiceCollection();

        services.AddCvSelectionOptions(
            configuration);

        using var serviceProvider =
            services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<
                IOptions<CvSelectionOptions>>()
            .Value;

        Assert.True(options.Enabled);
        Assert.True(options.RequireManualApproval);

        var profile = Assert.Single(
            options.Profiles);

        Assert.Equal(
            "DotNetDeveloper",
            profile.Name);

        Assert.Equal(
            110,
            profile.Priority);

        Assert.Equal(
            ".net developer",
            Assert.Single(profile.TitleKeywords));
    }

    [Fact]
    public void AddCvSelectionOptions_WhenSectionIsMissing_UsesDisabledDefaults()
    {
        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

        var services = new ServiceCollection();

        services.AddCvSelectionOptions(
            configuration);

        using var serviceProvider =
            services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<
                IOptions<CvSelectionOptions>>()
            .Value;

        Assert.False(options.Enabled);
        Assert.True(options.RequireManualApproval);
        Assert.Empty(options.Profiles);
    }

    [Fact]
    public void AddCvSelectionOptions_WhenEnabledConfigurationIsInvalid_Throws()
    {
        var values = new Dictionary<string, string?>
        {
            ["CvSelection:Enabled"] = "true",
            ["CvSelection:RequireManualApproval"] = "true"
        };

        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();

        var services = new ServiceCollection();

        services.AddCvSelectionOptions(
            configuration);

        using var serviceProvider =
            services.BuildServiceProvider();

        var exception = Assert.Throws<
            OptionsValidationException>(
                () => serviceProvider
                    .GetRequiredService<
                        IOptions<CvSelectionOptions>>()
                    .Value);

        Assert.Contains(
            exception.Failures,
            failure => failure.Contains(
                "must contain at least one CV profile",
                StringComparison.Ordinal));
    }
}
