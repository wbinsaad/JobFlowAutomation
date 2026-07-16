using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Application.Services;
using JobFlowAutomation.Configuration;
using JobFlowAutomation.Hosting.Workers;
using JobFlowAutomation.Infrastructure.Persistence;
using JobFlowAutomation.Infrastructure.Seek;
using JobFlowAutomation.Infrastructure.Selenium;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/JobFlowAutomation-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.AddJsonFile(
        "appsettings.Local.json",
        optional: true,
        reloadOnChange: false);

    // Restore production-friendly precedence:
    // environment variables and command-line values
    // must override local JSON configuration.
    builder.Configuration.AddEnvironmentVariables();

    if (args.Length > 0)
    {
        builder.Configuration.AddCommandLine(args);
    }

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger, dispose: true);

    builder.Services.Configure<SeekScraperOptions>(
        builder.Configuration.GetSection(
            SeekScraperOptions.ConfigurationSectionName));

    builder.Services.AddCvSelectionOptions(
    builder.Configuration);

    var connectionString = builder.Configuration.GetConnectionString("JobFlowDatabase");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'JobFlowDatabase' is missing.");
    }

    builder.Services.AddDbContextFactory<JobFlowDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddScoped<IWebDriver>(
        _ => WebDriverFactory.Create());

    builder.Services.AddScoped<SeleniumNavigator>();

    builder.Services.AddScoped<
        IJobScrapeStore,
        EfCoreJobScrapeStore>();

    builder.Services.AddTransient<SeekJobCardExtractor>();
    builder.Services.AddTransient<SeekJobListPageExtractor>();
    builder.Services.AddTransient<SeekJobDetailPageExtractor>();
    builder.Services.AddTransient<IJobScraper, SeekScraper>();

    builder.Services.AddScoped<
        IJobScrapingWorkflow,
        JobScrapingWorkflow>();

    builder.Services.AddHostedService<JobScrapingWorker>();

    using var host = builder.Build();

    // The database is initialized before JobScrapingWorker starts.
    await EnsureDatabaseCreatedAsync(host.Services);

    await host.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(
        exception,
        "Application crashed unexpectedly");

    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static async Task EnsureDatabaseCreatedAsync(
    IServiceProvider serviceProvider)
{
    var dbContextFactory =
        serviceProvider.GetRequiredService<
            IDbContextFactory<JobFlowDbContext>>();

    await using var dbContext =
        await dbContextFactory.CreateDbContextAsync();

    var databaseWasCreated =
        await dbContext.Database.EnsureCreatedAsync();

    if (databaseWasCreated)
    {
        Log.Information(
            "The JobFlow database and its schema were created successfully.");
    }
    else
    {
        Log.Information(
            "The JobFlow database already exists.");
    }
}
