# JobFlowAutomation

JobFlowAutomation is a .NET 10 Selenium-based job scraping and workflow automation project.

## Current architecture

- `Program.cs` is the composition root only.
- `JobScrapingWorker` controls the application flow.
- `IJobScraper` defines the scraper contract.
- `SeekScraper` implements the scraping pipeline.
- `JobScrapeResult` replaces tuple-based scrape results with a named model.
- Selenium services are registered through dependency injection.
- Logging uses `ILogger<T>` with Serilog console and rolling file sinks.
- Runtime settings are stored in `appsettings.json` and injected through the Options pattern.

## Run

```bash
dotnet restore
dotnet run
```

## Configuration

Edit `appsettings.json`:

```json
{
  "Scraper": {
    "Url": "https://au.seek.com/jobs-in-information-communication-technology/in-All-Melbourne-VIC?daterange=1&sortmode=ListedDate",
    "MinDelayMs": 3000,
    "MaxDelayMs": 8000,
    "WaitForKeyBeforeExit": true
  }
}
```

## Logging

Logs are written to:

```text
logs/JobFlowAutomation-YYYYMMDD.log
```
