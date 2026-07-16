namespace JobFlowAutomation.Configuration;

public sealed class CvProfileOptions
{
    public string Name { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public int Priority
    {
        get; set;
    }

    public List<string> TitleKeywords { get; set; } = [];

    public List<string> DescriptionKeywords { get; set; } = [];
}
