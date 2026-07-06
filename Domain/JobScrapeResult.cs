namespace JobFlowAutomation.Domain;

public sealed record JobScrapeResult(
    JobSummary Summary,
    ExtractionResult<JobDetail> Detail);
