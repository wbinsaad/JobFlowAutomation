namespace JobFlowAutomation.Infrastructure.Persistence;

public sealed class JobListingEntity
{
    public Guid Id { get; set; }

    public string CanonicalUrl { get; set; } = string.Empty;

    public string RawUrl { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string? Location { get; set; }

    public DateTimeOffset FirstSeenAtUtc { get; set; }

    public DateTimeOffset LastSeenAtUtc { get; set; }

    public List<JobScrapeRunEntity> ScrapeRuns { get; set; } = new();
}