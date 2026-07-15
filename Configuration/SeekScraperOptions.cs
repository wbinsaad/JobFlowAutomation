namespace JobFlowAutomation.Configuration;

public sealed class SeekScraperOptions
{
    public const string ConfigurationSectionName = "SeekScraperOptions";

    public string SearchPageUrl
    {
        get; init;
    } =
        "https://au.seek.com/jobs-in-information-communication-technology/in-All-Melbourne-VIC?daterange=1&sortmode=ListedDate";

    public int MinDelayMs { get; init; } = 3000;

    public int MaxDelayMs { get; init; } = 8000;

    public bool WaitForKeyBeforeExit
    {
        get; init;
    }
}
