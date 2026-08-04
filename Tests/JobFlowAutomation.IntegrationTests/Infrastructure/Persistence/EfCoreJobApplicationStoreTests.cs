using JobFlowAutomation.Application.Models;
using JobFlowAutomation.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace JobFlowAutomation.IntegrationTests.Infrastructure.Persistence;

[Collection(PostgreSqlDatabaseCollection.Name)]
public sealed class EfCoreJobApplicationStoreTests
    : IAsyncLifetime
{
    private static readonly DateTimeOffset
        s_initialTime =
            new(
                year: 2026,
                month: 8,
                day: 5,
                hour: 0,
                minute: 0,
                second: 0,
                offset: TimeSpan.Zero);

    private readonly PostgreSqlDatabaseFixture
        _databaseFixture;

    private readonly TestTimeProvider
        _timeProvider;

    public EfCoreJobApplicationStoreTests(
        PostgreSqlDatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;

        _timeProvider =
            new TestTimeProvider(
                s_initialTime);
    }

    public Task InitializeAsync()
    {
        return _databaseFixture
            .ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task
        CreateOrGet_WhenApplicationIsNew_InsertsRecordAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationDraft draft =
            CreateDraft();

        // Act
        JobApplicationCreateResult result =
            await store.CreateOrGetAsync(
                draft);

        // Assert
        Assert.True(result.WasCreated);

        Assert.NotEqual(
            Guid.Empty,
            result.Application.Id);

        Assert.Equal(
            JobApplicationStatus.AwaitingReview,
            result.Application.Status);

        Assert.Equal(
            "DotNetDeveloper",
            result.Application.SelectedCvProfile);

        Assert.Equal(
            "Waleed_Binsaad_DotNet_Developer_CV.pdf",
            result.Application.SelectedCvFileName);

        Assert.Equal(
            120,
            result.Application.SelectionScore);

        Assert.Equal(
            s_initialTime,
            result.Application.CreatedAtUtc);

        Assert.Equal(
            s_initialTime,
            result.Application.UpdatedAtUtc);

        await using JobFlowDbContext dbContext =
            await _databaseFixture
                .DbContextFactory
                .CreateDbContextAsync();

        Assert.Equal(
            1,
            await dbContext.JobApplications
                .CountAsync());

        Assert.Equal(
            1,
            await dbContext.JobListings
                .CountAsync());
    }

    [Fact]
    public async Task
        CreateOrGet_WhenCanonicalUrlAlreadyExists_ReturnsExistingRecordAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationDraft draft =
            CreateDraft();

        JobApplicationCreateResult first =
            await store.CreateOrGetAsync(
                draft);

        // Act
        JobApplicationCreateResult second =
            await store.CreateOrGetAsync(
                draft);

        // Assert
        Assert.True(first.WasCreated);
        Assert.False(second.WasCreated);

        Assert.Equal(
            first.Application.Id,
            second.Application.Id);

        await using JobFlowDbContext dbContext =
            await _databaseFixture
                .DbContextFactory
                .CreateDbContextAsync();

        Assert.Equal(
            1,
            await dbContext.JobApplications
                .CountAsync());
    }

    [Fact]
    public async Task
        CreateOrGet_WhenCalledConcurrently_CreatesOnlyOneRecordAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationDraft draft =
            CreateDraft();

        // Act
        Task<JobApplicationCreateResult> firstTask =
            store.CreateOrGetAsync(
                draft);

        Task<JobApplicationCreateResult> secondTask =
            store.CreateOrGetAsync(
                draft);

        JobApplicationCreateResult[] results =
            await Task.WhenAll(
                firstTask,
                secondTask);

        // Assert
        Assert.Equal(
            results[0].Application.Id,
            results[1].Application.Id);

        Assert.Single(results, result => result.WasCreated);

        await using JobFlowDbContext dbContext =
            await _databaseFixture
                .DbContextFactory
                .CreateDbContextAsync();

        Assert.Equal(
            1,
            await dbContext.JobApplications
                .CountAsync());

        Assert.Equal(
            1,
            await dbContext.JobListings
                .CountAsync());
    }

    [Fact]
    public async Task
        Get_WhenRecordExists_ReturnsSelectionAndStatusAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationDraft draft =
            CreateDraft();

        JobApplicationCreateResult created =
            await store.CreateOrGetAsync(
                draft);

        // Act
        JobApplicationRecord? result =
            await store.GetByCanonicalUrlAsync(
                draft.JobUrl);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            created.Application.Id,
            result.Id);

        Assert.Equal(
            JobApplicationStatus.AwaitingReview,
            result.Status);

        Assert.Equal(
            "DotNetDeveloper",
            result.SelectedCvProfile);

        Assert.Equal(
            [".net developer"],
            result.MatchedTitleKeywords);

        Assert.Equal(
            ["c#", "asp.net core"],
            result.MatchedDescriptionKeywords);
    }

    [Fact]
    public async Task
        TryTransition_WhenAwaitingReviewIsApproved_UpdatesStatusAndTimestampAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationCreateResult created =
            await store.CreateOrGetAsync(
                CreateDraft());

        _timeProvider.Advance(
            TimeSpan.FromHours(1));

        DateTimeOffset expectedApprovalTime =
            _timeProvider.GetUtcNow();

        // Act
        JobApplicationTransitionResult result =
            await store.TryTransitionAsync(
                created.Application.Id,
                JobApplicationStatus.Approved);

        // Assert
        Assert.Equal(
            JobApplicationTransitionOutcome.Updated,
            result.Outcome);

        Assert.NotNull(result.Application);

        Assert.Equal(
            JobApplicationStatus.Approved,
            result.Application.Status);

        Assert.Equal(
            expectedApprovalTime,
            result.Application.ApprovedAtUtc);

        Assert.Equal(
            expectedApprovalTime,
            result.Application.UpdatedAtUtc);
    }

    [Fact]
    public async Task
        TryTransition_WhenAwaitingReviewIsSkipped_UpdatesStatusAndTimestampAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationCreateResult created =
            await store.CreateOrGetAsync(
                CreateDraft());

        _timeProvider.Advance(
            TimeSpan.FromMinutes(30));

        DateTimeOffset expectedSkippedTime =
            _timeProvider.GetUtcNow();

        // Act
        JobApplicationTransitionResult result =
            await store.TryTransitionAsync(
                created.Application.Id,
                JobApplicationStatus.Skipped);

        // Assert
        Assert.Equal(
            JobApplicationTransitionOutcome.Updated,
            result.Outcome);

        Assert.NotNull(result.Application);

        Assert.Equal(
            JobApplicationStatus.Skipped,
            result.Application.Status);

        Assert.Equal(
            expectedSkippedTime,
            result.Application.SkippedAtUtc);
    }

    [Fact]
    public async Task
        TryTransition_WhenPreviousStatusIsFailed_AllowsExplicitRetryAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationCreateResult created =
            await store.CreateOrGetAsync(
                CreateDraft());

        JobApplicationTransitionResult failed =
            await store.TryTransitionAsync(
                created.Application.Id,
                JobApplicationStatus.Failed,
                failureCode: "FORM_UNAVAILABLE",
                failureMessage:
                    "The application form was unavailable.");

        Assert.Equal(
            JobApplicationTransitionOutcome.Updated,
            failed.Outcome);

        _timeProvider.Advance(
            TimeSpan.FromMinutes(10));

        // Act
        JobApplicationTransitionResult retried =
            await store.TryTransitionAsync(
                created.Application.Id,
                JobApplicationStatus.AwaitingReview);

        // Assert
        Assert.Equal(
            JobApplicationTransitionOutcome.Updated,
            retried.Outcome);

        Assert.NotNull(retried.Application);

        Assert.Equal(
            JobApplicationStatus.AwaitingReview,
            retried.Application.Status);
    }

    [Fact]
    public async Task
        TryTransition_WhenApplicationIsSubmitted_RejectsFurtherTransitionsAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationCreateResult created =
            await store.CreateOrGetAsync(
                CreateDraft());

        await store.TryTransitionAsync(
            created.Application.Id,
            JobApplicationStatus.Approved);

        await store.TryTransitionAsync(
            created.Application.Id,
            JobApplicationStatus.Prepared);

        JobApplicationTransitionResult submitted =
            await store.TryTransitionAsync(
                created.Application.Id,
                JobApplicationStatus.Submitted);

        Assert.Equal(
            JobApplicationTransitionOutcome.Updated,
            submitted.Outcome);

        // Act
        JobApplicationTransitionResult result =
            await store.TryTransitionAsync(
                created.Application.Id,
                JobApplicationStatus.AwaitingReview);

        // Assert
        Assert.Equal(
            JobApplicationTransitionOutcome.InvalidTransition,
            result.Outcome);

        Assert.NotNull(result.Application);

        Assert.Equal(
            JobApplicationStatus.Submitted,
            result.Application.Status);
    }

    [Fact]
    public async Task
        CreateOrGet_DoesNotPersistFullCvPathAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationDraft draft =
            CreateDraft();

        // Act
        JobApplicationCreateResult result =
            await store.CreateOrGetAsync(
                draft);

        // Assert
        Assert.Equal(
            "Waleed_Binsaad_DotNet_Developer_CV.pdf",
            result.Application.SelectedCvFileName);

        Assert.DoesNotContain(
            @"C:\Users\TestUser\Documents\CVs",
            result.Application.SelectedCvFileName,
            StringComparison.OrdinalIgnoreCase);

        await using JobFlowDbContext dbContext =
            await _databaseFixture
                .DbContextFactory
                .CreateDbContextAsync();

        JobApplicationEntity entity =
            await dbContext.JobApplications
                .SingleAsync();

        Assert.Equal(
            "Waleed_Binsaad_DotNet_Developer_CV.pdf",
            entity.SelectedCvFileName);

        Assert.DoesNotContain(
            @"C:\Users\TestUser",
            entity.SelectedCvFileName,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task
        Save_WhenTwoContextsUpdateSameRecord_ThrowsConcurrencyExceptionAsync()
    {
        // Arrange
        EfCoreJobApplicationStore store =
            CreateStore();

        JobApplicationCreateResult created =
            await store.CreateOrGetAsync(
                CreateDraft());

        await using JobFlowDbContext firstContext =
            await _databaseFixture
                .DbContextFactory
                .CreateDbContextAsync();

        await using JobFlowDbContext secondContext =
            await _databaseFixture
                .DbContextFactory
                .CreateDbContextAsync();

        JobApplicationEntity firstEntity =
            await firstContext.JobApplications
                .SingleAsync(
                    application =>
                        application.Id
                        == created.Application.Id);

        JobApplicationEntity secondEntity =
            await secondContext.JobApplications
                .SingleAsync(
                    application =>
                        application.Id
                        == created.Application.Id);

        firstEntity.Status =
            JobApplicationStatus.Approved;

        firstEntity.ApprovedAtUtc =
            _timeProvider.GetUtcNow();

        firstEntity.UpdatedAtUtc =
            _timeProvider.GetUtcNow();

        await firstContext.SaveChangesAsync();

        secondEntity.Status =
            JobApplicationStatus.Skipped;

        secondEntity.SkippedAtUtc =
            _timeProvider.GetUtcNow();

        secondEntity.UpdatedAtUtc =
            _timeProvider.GetUtcNow();

        // Act
        Task action =
            secondContext.SaveChangesAsync();

        // Assert
        await Assert.ThrowsAsync<
            DbUpdateConcurrencyException>(
                () => action);
    }

    private EfCoreJobApplicationStore CreateStore()
    {
        return new EfCoreJobApplicationStore(
            _databaseFixture.DbContextFactory,
            _timeProvider,
            NullLogger<EfCoreJobApplicationStore>
                .Instance);
    }

    private static JobApplicationDraft CreateDraft()
    {
        var request =
            new ApplicationPreparationRequest(
                JobTitle: ".NET Developer",
                JobDescription:
                    "Build services using C# and ASP.NET Core.",
                Company: "Example Technology",
                JobUrl: new Uri(
                    "https://www.seek.com.au/job/12345678"));

        var selection =
            new CvSelectionResult(
                ProfileName: "DotNetDeveloper",
                FilePath:
                    @"C:\Users\TestUser\Documents\CVs\Waleed_Binsaad_DotNet_Developer_CV.pdf",
                Score: 120,
                MatchedTitleKeywords:
                [
                    ".net developer"
                ],
                MatchedDescriptionKeywords:
                [
                    "c#",
                    "asp.net core"
                ],
                RequiresManualApproval: true);

        CvFileValidationResult fileValidation =
            CvFileValidationResult.Success(
                selection.FilePath);

        ApplicationPreparationResult preparation =
            ApplicationPreparationResult
                .AwaitingReview(
                    selection,
                    fileValidation);

        return JobApplicationDraft.FromPreparation(
            request,
            preparation);
    }
}
