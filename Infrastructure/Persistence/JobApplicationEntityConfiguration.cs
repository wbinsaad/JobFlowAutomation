using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobFlowAutomation.Infrastructure.Persistence;

public sealed class JobApplicationEntityConfiguration
    : IEntityTypeConfiguration<JobApplicationEntity>
{
    public void Configure(EntityTypeBuilder<JobApplicationEntity> entity)
    {
        entity.ToTable("job_applications");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.CanonicalJobUrl)
            .HasMaxLength(2048)
            .IsRequired();

        entity.HasIndex(x => x.CanonicalJobUrl)
            .IsUnique()
            .HasDatabaseName(
                "ux_job_applications_canonical_job_url");

        entity.HasIndex(x => x.JobListingId)
            .IsUnique()
            .HasDatabaseName(
                "ux_job_applications_job_listing_id");

        entity.Property(x => x.JobTitle)
            .HasMaxLength(512)
            .IsRequired();

        entity.Property(x => x.Company)
            .HasMaxLength(256);

        entity.Property(x => x.SelectedCvProfile)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.SelectedCvFileName)
            .HasMaxLength(512)
            .IsRequired();

        entity.Property(x => x.SelectionScore)
            .IsRequired();

        entity.Property(x => x.MatchedTitleKeywords)
            .HasColumnType("text[]")
            .IsRequired();

        entity.Property(
                x => x.MatchedDescriptionKeywords)
            .HasColumnType("text[]")
            .IsRequired();

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(x => x.RequiresManualApproval)
            .IsRequired();

        entity.Property(x => x.FailureCode)
            .HasMaxLength(128);

        entity.Property(x => x.FailureMessage)
            .HasMaxLength(2048);

        entity.Property(x => x.CreatedAtUtc)
            .IsRequired();

        entity.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        entity.Property(x => x.Version)
            .IsRowVersion();

        entity.HasOne(x => x.JobListing)
            .WithOne()
            .HasForeignKey<JobApplicationEntity>(
                x => x.JobListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
