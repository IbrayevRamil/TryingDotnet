using TryingDotnetTests.DataAccess;
using TryingDotnetTests.Events;

namespace TryingDotnetTests;

[CollectionDefinition(nameof(ContainersCollection))]
public class ContainersCollection : ICollectionFixture<DatabaseFixture>, ICollectionFixture<KafkaFixture>;