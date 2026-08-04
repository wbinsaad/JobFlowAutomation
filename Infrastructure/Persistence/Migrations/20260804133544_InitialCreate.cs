using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFlowAutomation.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "job_listings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CanonicalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                RawUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                Company = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                FirstSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_job_listings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "job_applications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JobListingId = table.Column<Guid>(type: "uuid", nullable: false),
                CanonicalJobUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                JobTitle = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                Company = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                SelectedCvProfile = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                SelectedCvFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                SelectionScore = table.Column<int>(type: "integer", nullable: false),
                MatchedTitleKeywords = table.Column<string[]>(type: "text[]", nullable: false),
                MatchedDescriptionKeywords = table.Column<string[]>(type: "text[]", nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                RequiresManualApproval = table.Column<bool>(type: "boolean", nullable: false),
                ApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                PreparedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                SkippedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                RejectedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                FailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                FailureCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                FailureMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_job_applications", x => x.Id);
                table.ForeignKey(
                    name: "FK_job_applications_job_listings_JobListingId",
                    column: x => x.JobListingId,
                    principalTable: "job_listings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "job_scrape_runs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JobListingId = table.Column<Guid>(type: "uuid", nullable: false),
                CanonicalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                RawUrl = table.Column<string>(type: "text", nullable: false),
                Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                Company = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                AdvertiserName = table.Column<string>(type: "text", nullable: true),
                Classifications = table.Column<string>(type: "text", nullable: true),
                Salary = table.Column<string>(type: "text", nullable: true),
                WorkType = table.Column<string>(type: "text", nullable: true),
                Description = table.Column<string>(type: "text", nullable: true),
                IsQuickApply = table.Column<bool>(type: "boolean", nullable: false),
                DetailSucceeded = table.Column<bool>(type: "boolean", nullable: false),
                ScrapedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_job_scrape_runs", x => x.Id);
                table.ForeignKey(
                    name: "FK_job_scrape_runs_job_listings_JobListingId",
                    column: x => x.JobListingId,
                    principalTable: "job_listings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ux_job_applications_canonical_job_url",
            table: "job_applications",
            column: "CanonicalJobUrl",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_job_applications_job_listing_id",
            table: "job_applications",
            column: "JobListingId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_job_listings_CanonicalUrl",
            table: "job_listings",
            column: "CanonicalUrl",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_job_scrape_runs_JobListingId_ScrapedAtUtc",
            table: "job_scrape_runs",
            columns: new[] { "JobListingId", "ScrapedAtUtc" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "job_applications");

        migrationBuilder.DropTable(
            name: "job_scrape_runs");

        migrationBuilder.DropTable(
            name: "job_listings");
    }
}
