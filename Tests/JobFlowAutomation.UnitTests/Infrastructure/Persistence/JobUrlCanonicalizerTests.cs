using JobFlowAutomation.Infrastructure.Persistence;

namespace JobFlowAutomation.UnitTests.Infrastructure.Persistence;

public sealed class JobUrlCanonicalizerTests
{
    [Theory]
    [InlineData(
        "https://www.seek.com.au/job/12345?tracking=abc#details",
        "https://www.seek.com.au/job/12345")]
    [InlineData(
        "https://www.seek.com.au/job/12345#details",
        "https://www.seek.com.au/job/12345")]
    [InlineData(
        "https://www.seek.com.au/job/12345?tracking=abc",
        "https://www.seek.com.au/job/12345")]
    [InlineData(
        "https://www.seek.com.au/job/12345",
        "https://www.seek.com.au/job/12345")]
    public void Canonicalize_WhenUrlContainsTrackingInformation_RemovesQueryAndFragment(
        string inputUrl,
        string expectedUrl)
    {
        // Arrange
        var detailUrl = new Uri(inputUrl);

        // Act
        var actualUrl = JobUrlCanonicalizer.Canonicalize(detailUrl);

        // Assert
        Assert.Equal(expectedUrl, actualUrl);
    }
}
