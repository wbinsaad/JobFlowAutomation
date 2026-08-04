using JobFlowAutomation.Application.Models;

namespace JobFlowAutomation.Infrastructure.Persistence;

public sealed class JobApplicationEntity
{
    public Guid Id
    {
        get; set;
    }

    public Guid JobListingId
    {
        get; set;
    }

    public JobListingEntity JobListing
    {
        get; set;
    } = null!;

    public string CanonicalJobUrl
    {
        get; set;
    } = string.Empty;

    public string JobTitle
    {
        get; set;
    } = string.Empty;

    public string? Company
    {
        get; set;
    }

    public string SelectedCvProfile
    {
        get; set;
    } = string.Empty;

    public string SelectedCvFileName
    {
        get; set;
    } = string.Empty;

    public int SelectionScore
    {
        get; set;
    }

    public string[] MatchedTitleKeywords
    {
        get; set;
    } = [];

    public string[] MatchedDescriptionKeywords
    {
        get; set;
    } = [];

    public JobApplicationStatus Status
    {
        get; set;
    }

    public bool RequiresManualApproval
    {
        get; set;
    }

    public DateTimeOffset? ApprovedAtUtc
    {
        get; set;
    }

    public DateTimeOffset? PreparedAtUtc
    {
        get; set;
    }

    public DateTimeOffset? SubmittedAtUtc
    {
        get; set;
    }

    public DateTimeOffset? SkippedAtUtc
    {
        get; set;
    }

    public DateTimeOffset? RejectedAtUtc
    {
        get; set;
    }

    public DateTimeOffset? FailedAtUtc
    {
        get; set;
    }

    public string? FailureCode
    {
        get; set;
    }

    public string? FailureMessage
    {
        get; set;
    }

    public DateTimeOffset CreatedAtUtc
    {
        get; set;
    }

    public DateTimeOffset UpdatedAtUtc
    {
        get; set;
    }

    public uint Version
    {
        get; set;
    }
}
