using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JobFlowAutomation.Infrastructure.Persistence;

/// <summary>
/// Creates the database context for EF Core design-time commands,
/// including migrations and database updates.
/// </summary>
public sealed class JobFlowDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<JobFlowDbContext>
{
    public JobFlowDbContext CreateDbContext(
        string[] args)
    {
        string environmentName =
            Environment.GetEnvironmentVariable(
                "DOTNET_ENVIRONMENT")
            ?? Environments.Production;

        IConfigurationBuilder configurationBuilder =
            new ConfigurationBuilder()
                .SetBasePath(
                    Directory.GetCurrentDirectory())
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: false)
                .AddJsonFile(
                    $"appsettings.{environmentName}.json",
                    optional: true,
                    reloadOnChange: false)
                .AddJsonFile(
                    "appsettings.Local.json",
                    optional: true,
                    reloadOnChange: false)
                .AddEnvironmentVariables();

        if (args.Length > 0)
        {
            configurationBuilder.AddCommandLine(
                args);
        }

        IConfiguration configuration =
            configurationBuilder.Build();

        string connectionString =
            configuration.GetConnectionString(
                "JobFlowDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'JobFlowDatabase' "
                + "is missing.");

        var optionsBuilder =
            new DbContextOptionsBuilder<
                JobFlowDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString);

        return new JobFlowDbContext(
            optionsBuilder.Options);
    }
}
