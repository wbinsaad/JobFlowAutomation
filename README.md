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
- Scraped jobs are saved to PostgreSQL with canonical URL deduplication and scrape-run history.

## Run

```bash
dotnet restore
dotnet run
```

## Configuration

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "JobFlowDatabase": "Host=localhost;Port=5432;Database=JobFlowAutomation;Username=postgres;Password=postgres"
  },
  "SeekScraperOptions": {
    "SearchPageUrl": "https://au.seek.com/jobs-in-information-communication-technology/in-All-Melbourne-VIC?daterange=1&sortmode=ListedDate",
    "MinDelayMs": 3000,
    "MaxDelayMs": 8000,
    "WaitForKeyBeforeExit": true
  }
}
```

The application canonicalizes each Seek detail URL before saving it, so tracking query parameters do not create duplicate job rows.

## Supported CV files

Configured CV paths must:

- Be absolute paths.
- Point to an existing file.
- Use one of these extensions:
  - `.pdf`
  - `.doc`
  - `.docx`

Extension matching is case-insensitive.

The validator checks the path, file existence and extension. It does not
inspect the document contents or verify that the contents match the extension.
CV files and `appsettings.Local.json` must not be committed to source control.

## Logging

Logs are written to:

```text
logs/JobFlowAutomation-YYYYMMDD.log
```
