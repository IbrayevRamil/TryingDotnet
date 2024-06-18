using TryingDotnetTests.DataAccess;

namespace TryingDotnetTests;

[CollectionDefinition(nameof(ContainersCollection))]
public class ContainersCollection : ICollectionFixture<DatabaseFixture>;