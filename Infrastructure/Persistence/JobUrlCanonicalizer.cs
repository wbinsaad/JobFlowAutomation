namespace JobFlowAutomation.Infrastructure.Persistence;

public static class JobUrlCanonicalizer
{
    public static string Canonicalize(Uri detailUrl)
    {
        var builder = new UriBuilder(detailUrl)
        {
            Query = string.Empty,
            Fragment = string.Empty
        };

        return builder.Uri.GetLeftPart(UriPartial.Path);
    }
}
