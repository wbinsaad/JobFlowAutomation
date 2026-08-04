namespace JobFlowAutomation.IntegrationTests;

public sealed class TestTimeProvider
    : TimeProvider
{
    private DateTimeOffset _utcNow;

    public TestTimeProvider(
        DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }

    public void Advance(
        TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                "Time cannot move backwards.");
        }

        _utcNow = _utcNow.Add(duration);
    }
}
