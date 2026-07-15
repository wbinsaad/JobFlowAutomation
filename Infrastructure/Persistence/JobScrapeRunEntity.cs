namespace JobFlowAutomation.Infrastructure.Persistence;

public sealed class JobScrapeRunEntity
{
    public Guid Id
    {
        get; set;
    }

    public Guid JobListingId
    {
        get; set;
    }

    public JobListingEntity JobListing { get; set; } = null!;

    public string CanonicalUrl { get; set; } = string.Empty;

    public string RawUrl { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Company
    {
        get; set;
    }

    public string? Location
    {
        get; set;
    }

    public string? AdvertiserName
    {
        get; set;
    }

    public string? Classifications
    {
        get; set;
    }

    public string? Salary
    {
        get; set;
    }

    public string? WorkType
    {
        get; set;
    }

    public string? Description
    {
        get; set;
    }

    public bool IsQuickApply
    {
        get; set;
    }

    public bool DetailSucceeded
    {
        get; set;
    }

    public DateTimeOffset ScrapedAtUtc
    {
        get; set;
    }
}
