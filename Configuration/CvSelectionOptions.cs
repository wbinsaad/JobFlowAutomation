namespace JobFlowAutomation.Configuration;

public sealed class CvSelectionOptions
{
    public const string ConfigurationSectionName = "CvSelection";

    public bool Enabled
    {
        get; set;
    }

    public bool RequireManualApproval { get; set; } = true;

    public List<CvProfileOptions> Profiles { get; set; } = [];
}
