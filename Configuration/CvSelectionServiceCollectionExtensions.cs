using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JobFlowAutomation.Configuration;

public static class CvSelectionServiceCollectionExtensions
{
    public static IServiceCollection AddCvSelectionOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<
            IValidateOptions<CvSelectionOptions>,
            CvSelectionOptionsValidator>();

        services
            .AddOptions<CvSelectionOptions>()
            .Bind(
                configuration.GetSection(
                    CvSelectionOptions.ConfigurationSectionName))
            .ValidateOnStart();

        return services;
    }
}
