using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Application.Services;
using JobFlowAutomation.Configuration;
using JobFlowAutomation.Infrastructure.Seek;
using JobFlowAutomation.Infrastructure.Selenium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/JobFlowAutomation-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger, dispose: true);

    builder.Services.Configure<ScraperOptions>(
        builder.Configuration.GetSection(ScraperOptions.SectionName));

    builder.Services.AddSingleton<IWebDriver>(_ => WebDriverFactory.Create());
    builder.Services.AddSingleton<SeleniumNavigator>();

    builder.Services.AddTransient<SeekJobCardExtractor>();
    builder.Services.AddTransient<SeekJobListPageExtractor>();
    builder.Services.AddTransient<SeekJobDetailPageExtractor>();
    builder.Services.AddTransient<IJobScraper, SeekScraper>();

    builder.Services.AddHostedService<JobScrapingWorker>();

    using var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application crashed unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
