using JobFlowAutomation.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.PostgreSql;

namespace JobFlowAutomation.IntegrationTests.Infrastructure.Persistence;

public sealed class PostgreSqlDatabaseFixture
    : IAsyncLifetime
{
    [Obsolete]
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .WithImage("postgres:18.4")
            .WithDatabase("jobflow_automation_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    private ServiceProvider? _serviceProvider;

    public IDbContextFactory<JobFlowDbContext>
        DbContextFactory =>
        _serviceProvider?
            .GetRequiredService<
                IDbContextFactory<JobFlowDbContext>>()
        ?? throw new InvalidOperationException(
            "The PostgreSQL fixture has not been initialized.");

    [Obsolete]
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContextFactory<JobFlowDbContext>(
            options =>
                options.UseNpgsql(
                    _container.GetConnectionString()));

        _serviceProvider =
            services.BuildServiceProvider(
                validateScopes: true);

        await using JobFlowDbContext dbContext =
            await DbContextFactory
                .CreateDbContextAsync();

        await dbContext.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using JobFlowDbContext dbContext =
            await DbContextFactory
                .CreateDbContextAsync();

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE
                job_applications,
                job_scrape_runs,
                job_listings
            CASCADE;
            """);
    }

    [Obsolete]
    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}
