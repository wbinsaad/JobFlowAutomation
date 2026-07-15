using Microsoft.EntityFrameworkCore;

namespace JobFlowAutomation.Infrastructure.Persistence;

public sealed class JobFlowDbContext : DbContext
{
    public JobFlowDbContext(DbContextOptions<JobFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobListingEntity> JobListings => Set<JobListingEntity>();

    public DbSet<JobScrapeRunEntity> JobScrapeRuns => Set<JobScrapeRunEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobListingEntity>(entity =>
        {
            entity.ToTable("job_listings");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CanonicalUrl)
                .HasMaxLength(2048)
                .IsRequired();

            entity.HasIndex(x => x.CanonicalUrl)
                .IsUnique();

            entity.Property(x => x.RawUrl)
                .HasMaxLength(2048)
                .IsRequired();

            entity.Property(x => x.Title)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(x => x.Company)
                .HasMaxLength(256);

            entity.Property(x => x.Location)
                .HasMaxLength(256);

            entity.Property(x => x.FirstSeenAtUtc)
                .IsRequired();

            entity.Property(x => x.LastSeenAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<JobScrapeRunEntity>(entity =>
        {
            entity.ToTable("job_scrape_runs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CanonicalUrl)
                .HasMaxLength(2048)
                .IsRequired();

            entity.Property(x => x.Title)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(x => x.Company)
                .HasMaxLength(256);

            entity.Property(x => x.Location)
                .HasMaxLength(256);

            entity.Property(x => x.ScrapedAtUtc)
                .IsRequired();

            entity.Property(x => x.DetailSucceeded)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasColumnType("text");

            entity.HasIndex(x => new { x.JobListingId, x.ScrapedAtUtc });

            entity.HasOne(x => x.JobListing)
                .WithMany(x => x.ScrapeRuns)
                .HasForeignKey(x => x.JobListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}