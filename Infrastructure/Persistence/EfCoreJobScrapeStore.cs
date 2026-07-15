using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobFlowAutomation.Infrastructure.Persistence;

public sealed class EfCoreJobScrapeStore : IJobScrapeStore
{
    private readonly IDbContextFactory<JobFlowDbContext> _contextFactory;
    private readonly ILogger<EfCoreJobScrapeStore> _logger;

    public EfCoreJobScrapeStore(
        IDbContextFactory<JobFlowDbContext> contextFactory,
        ILogger<EfCoreJobScrapeStore> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SaveScrapeResultsAsync(IReadOnlyList<JobScrapeResult> scrapeResults, CancellationToken cancellationToken = default)
    {
        if (scrapeResults.Count == 0)
        {
            return;
        }

        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var scrapedAtUtc = DateTimeOffset.UtcNow;

        foreach (var result in scrapeResults)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var canonicalUrl = JobUrlCanonicalizer.Canonicalize(result.Summary.DetailUrl);
            var rawUrl = result.Summary.DetailUrl.ToString();
            var existingJobListing = await dbContext.JobListings
                .FirstOrDefaultAsync(x => x.CanonicalUrl == canonicalUrl, cancellationToken);

            if (existingJobListing is null)
            {
                existingJobListing = new JobListingEntity
                {
                    Id = Guid.NewGuid(),
                    CanonicalUrl = canonicalUrl,
                    RawUrl = rawUrl,
                    Title = result.Summary.Title,
                    Company = result.Summary.AdvertiserName,
                    Location = result.Summary.Location,
                    FirstSeenAtUtc = scrapedAtUtc,
                    LastSeenAtUtc = scrapedAtUtc
                };

                dbContext.JobListings.Add(existingJobListing);
            }
            else
            {
                existingJobListing.LastSeenAtUtc = scrapedAtUtc;
            }

            var detailSucceeded = result.Detail.IsSuccessful && result.Detail.Value is not null;
            var detail = result.Detail.Value;

            dbContext.JobScrapeRuns.Add(new JobScrapeRunEntity
            {
                Id = Guid.NewGuid(),
                JobListing = existingJobListing,
                CanonicalUrl = canonicalUrl,
                RawUrl = rawUrl,
                Title = result.Summary.Title,
                Company = result.Summary.AdvertiserName,
                Location = result.Summary.Location,
                AdvertiserName = detail?.AdvertiserName,
                Classifications = detail?.Classifications,
                Salary = detail?.Salary,
                WorkType = detail?.WorkType,
                Description = detail?.Description,
                IsQuickApply = detail?.IsQuickApply ?? false,
                DetailSucceeded = detailSucceeded,
                ScrapedAtUtc = scrapedAtUtc
            });

            if (!detailSucceeded)
            {
                _logger.LogWarning("Persisting partial scrape for {Title} at {Url}", result.Summary.Title, rawUrl);
            }
            else
            {
                _logger.LogInformation("Persisting scrape for {Title} at {Url}", result.Summary.Title, rawUrl);
            }
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Duplicate job detected while saving scrape batch");
            throw;
        }
    }
}