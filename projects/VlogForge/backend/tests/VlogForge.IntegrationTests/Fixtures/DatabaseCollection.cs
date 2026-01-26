using Xunit;

namespace VlogForge.IntegrationTests.Fixtures;

/// <summary>
/// Collection definition for database integration tests.
/// Tests in this collection share the same database container.
/// </summary>
[CollectionDefinition("Database")]
public class IntegrationTestsCollectionDefinition : ICollectionFixture<WebApplicationFactoryFixture>
{
}
