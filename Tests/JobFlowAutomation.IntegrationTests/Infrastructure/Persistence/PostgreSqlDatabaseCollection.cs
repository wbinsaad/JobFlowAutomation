namespace JobFlowAutomation.IntegrationTests.Infrastructure.Persistence;

[CollectionDefinition(
    Name,
    DisableParallelization = true)]
public sealed class PostgreSqlDatabaseCollection
    : ICollectionFixture<PostgreSqlDatabaseFixture>
{
    public const string Name =
        "PostgreSQL database collection";
}
