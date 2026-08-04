using JobFlowAutomation.Application.Abstractions;
using JobFlowAutomation.Application.Models;
using JobFlowAutomation.Application.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace JobFlowAutomation.Infrastructure.Persistence;

public sealed partial class EfCoreJobApplicationStore
    : IJobApplicationStore
{
    private const int MaximumCreateAttempts = 2;

    private readonly IDbContextFactory<JobFlowDbContext>
        _contextFactory;

    private readonly TimeProvider _timeProvider;

    private readonly ILogger<EfCoreJobApplicationStore>
        _logger;

    public EfCoreJobApplicationStore(
        IDbContextFactory<JobFlowDbContext>
            contextFactory,
        TimeProvider timeProvider,
        ILogger<EfCoreJobApplicationStore> logger)
    {
        ArgumentNullException.ThrowIfNull(
            contextFactory);

        ArgumentNullException.ThrowIfNull(
            timeProvider);

        ArgumentNullException.ThrowIfNull(
            logger);

        _contextFactory = contextFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<JobApplicationCreateResult>
        CreateOrGetAsync(
            JobApplicationDraft draft,
            CancellationToken cancellationToken =
                default)
    {
        ArgumentNullException.ThrowIfNull(draft);

        string canonicalUrl =
            JobUrlCanonicalizer.Canonicalize(
                draft.JobUrl);

        for (var attempt = 0;
             attempt < MaximumCreateAttempts;
             attempt++)
        {
            await using JobFlowDbContext dbContext =
                await _contextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            JobApplicationEntity? existing =
                await FindByCanonicalUrlAsync(
                    dbContext,
                    canonicalUrl,
                    tracking: false,
                    cancellationToken);

            if (existing is not null)
            {
                return new JobApplicationCreateResult(
                    Map(existing),
                    WasCreated: false);
            }

            DateTimeOffset now =
                _timeProvider.GetUtcNow();

            JobListingEntity? jobListing =
                await dbContext.JobListings
                    .SingleOrDefaultAsync(
                        listing =>
                            listing.CanonicalUrl
                            == canonicalUrl,
                        cancellationToken);

            if (jobListing is null)
            {
                jobListing = new JobListingEntity
                {
                    Id = Guid.NewGuid(),
                    CanonicalUrl = canonicalUrl,
                    RawUrl =
                        draft.JobUrl.ToString(),
                    Title = draft.JobTitle,
                    Company = draft.Company,
                    Location = null,
                    FirstSeenAtUtc = now,
                    LastSeenAtUtc = now
                };

                dbContext.JobListings.Add(
                    jobListing);
            }

            var entity =
                new JobApplicationEntity
                {
                    Id = Guid.NewGuid(),
                    JobListing = jobListing,
                    CanonicalJobUrl =
                        canonicalUrl,
                    JobTitle = draft.JobTitle,
                    Company = draft.Company,
                    SelectedCvProfile =
                        draft.SelectedCvProfile,
                    SelectedCvFileName =
                        draft.SelectedCvFileName,
                    SelectionScore =
                        draft.SelectionScore,
                    MatchedTitleKeywords =
                        draft.MatchedTitleKeywords
                            .ToArray(),
                    MatchedDescriptionKeywords =
                        draft
                            .MatchedDescriptionKeywords
                            .ToArray(),
                    Status =
                        JobApplicationStatus
                            .AwaitingReview,
                    RequiresManualApproval =
                        draft
                            .RequiresManualApproval,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };

            dbContext.JobApplications.Add(entity);

            try
            {
                await dbContext.SaveChangesAsync(
                    cancellationToken);

                LogApplicationCreated(
                    entity.Id,
                    canonicalUrl);

                return new JobApplicationCreateResult(
                    Map(entity),
                    WasCreated: true);
            }
            catch (DbUpdateException exception)
                when (IsUniqueViolation(exception))
            {
                LogDuplicateDetected(canonicalUrl);

                JobApplicationRecord? concurrent =
                    await GetByCanonicalUrlCoreAsync(
                        canonicalUrl,
                        cancellationToken);

                if (concurrent is not null)
                {
                    return new JobApplicationCreateResult(
                        concurrent,
                        WasCreated: false);
                }

                if (attempt
                    == MaximumCreateAttempts - 1)
                {
                    throw;
                }
            }
        }

        throw new InvalidOperationException(
            "The application could not be created.");
    }

    public async Task<JobApplicationRecord?>
        GetByCanonicalUrlAsync(
            Uri jobUrl,
            CancellationToken cancellationToken =
                default)
    {
        ArgumentNullException.ThrowIfNull(jobUrl);

        string canonicalUrl =
            JobUrlCanonicalizer.Canonicalize(
                jobUrl);

        return await GetByCanonicalUrlCoreAsync(
            canonicalUrl,
            cancellationToken);
    }

    public async Task<JobApplicationTransitionResult>
        TryTransitionAsync(
            Guid applicationId,
            JobApplicationStatus targetStatus,
            string? failureCode = null,
            string? failureMessage = null,
            CancellationToken cancellationToken =
                default)
    {
        if (applicationId == Guid.Empty)
        {
            return new JobApplicationTransitionResult(
                JobApplicationTransitionOutcome
                    .InvalidRequest,
                Application: null,
                "Application ID is required.");
        }

        if (targetStatus
            == JobApplicationStatus.Unknown)
        {
            return new JobApplicationTransitionResult(
                JobApplicationTransitionOutcome
                    .InvalidRequest,
                Application: null,
                "Target status is required.");
        }

        if (targetStatus
                == JobApplicationStatus.Failed
            && string.IsNullOrWhiteSpace(
                failureCode))
        {
            return new JobApplicationTransitionResult(
                JobApplicationTransitionOutcome
                    .InvalidRequest,
                Application: null,
                "A failure code is required "
                + "for the Failed status.");
        }

        await using JobFlowDbContext dbContext =
            await _contextFactory
                .CreateDbContextAsync(
                    cancellationToken);

        JobApplicationEntity? entity =
            await dbContext.JobApplications
                .SingleOrDefaultAsync(
                    application =>
                        application.Id
                        == applicationId,
                    cancellationToken);

        if (entity is null)
        {
            return new JobApplicationTransitionResult(
                JobApplicationTransitionOutcome
                    .NotFound,
                Application: null,
                "The job application was not found.");
        }

        if (!JobApplicationStatusPolicy
                .CanTransition(
                    entity.Status,
                    targetStatus))
        {
            return new JobApplicationTransitionResult(
                JobApplicationTransitionOutcome
                    .InvalidTransition,
                Map(entity),
                $"Transition from {entity.Status} "
                + $"to {targetStatus} is not allowed.");
        }

        DateTimeOffset now =
            _timeProvider.GetUtcNow();

        ApplyTransition(
            entity,
            targetStatus,
            failureCode,
            failureMessage,
            now);

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            LogConcurrencyConflict(
                applicationId);

            return new JobApplicationTransitionResult(
                JobApplicationTransitionOutcome
                    .ConcurrencyConflict,
                Application: null,
                "The application was modified "
                + "by another operation.");
        }

        return new JobApplicationTransitionResult(
            JobApplicationTransitionOutcome.Updated,
            Map(entity),
            Message: null);
    }

    private async Task<JobApplicationRecord?>
        GetByCanonicalUrlCoreAsync(
            string canonicalUrl,
            CancellationToken cancellationToken)
    {
        await using JobFlowDbContext dbContext =
            await _contextFactory
                .CreateDbContextAsync(
                    cancellationToken);

        JobApplicationEntity? entity =
            await FindByCanonicalUrlAsync(
                dbContext,
                canonicalUrl,
                tracking: false,
                cancellationToken);

        return entity is null
            ? null
            : Map(entity);
    }

    private static Task<JobApplicationEntity?>
        FindByCanonicalUrlAsync(
            JobFlowDbContext dbContext,
            string canonicalUrl,
            bool tracking,
            CancellationToken cancellationToken)
    {
        IQueryable<JobApplicationEntity> query =
            dbContext.JobApplications;

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(
            application =>
                application.CanonicalJobUrl
                == canonicalUrl,
            cancellationToken);
    }

    private static void ApplyTransition(
        JobApplicationEntity entity,
        JobApplicationStatus targetStatus,
        string? failureCode,
        string? failureMessage,
        DateTimeOffset now)
    {
        entity.Status = targetStatus;
        entity.UpdatedAtUtc = now;

        switch (targetStatus)
        {
            case JobApplicationStatus.Approved:
                entity.ApprovedAtUtc = now;
                break;

            case JobApplicationStatus.Prepared:
                entity.PreparedAtUtc = now;
                break;

            case JobApplicationStatus.Submitted:
                entity.SubmittedAtUtc = now;
                break;

            case JobApplicationStatus.Skipped:
                entity.SkippedAtUtc = now;
                break;

            case JobApplicationStatus.Rejected:
                entity.RejectedAtUtc = now;
                break;

            case JobApplicationStatus.Failed:
                entity.FailedAtUtc = now;
                entity.FailureCode =
                    failureCode?.Trim();
                entity.FailureMessage =
                    string.IsNullOrWhiteSpace(
                        failureMessage)
                        ? null
                        : failureMessage.Trim();
                break;
        }
    }

    private static bool IsUniqueViolation(
        DbUpdateException exception)
    {
        return exception.InnerException
            is PostgresException postgresException
            && postgresException.SqlState
            == PostgresErrorCodes.UniqueViolation;
    }

    private static JobApplicationRecord Map(
        JobApplicationEntity entity)
    {
        return new JobApplicationRecord(
            entity.Id,
            entity.JobListingId,
            entity.CanonicalJobUrl,
            entity.JobTitle,
            entity.Company,
            entity.SelectedCvProfile,
            entity.SelectedCvFileName,
            entity.SelectionScore,
            Array.AsReadOnly(
                entity.MatchedTitleKeywords
                    .ToArray()),
            Array.AsReadOnly(
                entity
                    .MatchedDescriptionKeywords
                    .ToArray()),
            entity.Status,
            entity.RequiresManualApproval,
            entity.ApprovedAtUtc,
            entity.PreparedAtUtc,
            entity.SubmittedAtUtc,
            entity.SkippedAtUtc,
            entity.RejectedAtUtc,
            entity.FailedAtUtc,
            entity.FailureCode,
            entity.FailureMessage,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.Version);
    }

    [LoggerMessage(
        EventId = 2300,
        Level = LogLevel.Information,
        Message =
            "Created job application {ApplicationId} "
            + "for canonical URL {CanonicalUrl}.")]
    private partial void LogApplicationCreated(
        Guid applicationId,
        string canonicalUrl);

    [LoggerMessage(
        EventId = 2301,
        Level = LogLevel.Debug,
        Message =
            "A concurrent or duplicate application "
            + "was detected for {CanonicalUrl}.")]
    private partial void LogDuplicateDetected(
        string canonicalUrl);

    [LoggerMessage(
        EventId = 2302,
        Level = LogLevel.Warning,
        Message =
            "A concurrency conflict occurred while "
            + "updating job application {ApplicationId}.")]
    private partial void LogConcurrencyConflict(
        Guid applicationId);
}
