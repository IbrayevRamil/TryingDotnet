using Microsoft.AspNetCore.Mvc.Testing;
using Refit;
using TryingDotnet.Api;
using TryingDotnetTests.DataAccess;
using TryingDotnetTests.Events;

namespace TryingDotnetTests;

[Collection(nameof(ContainersCollection))]
public class GenericIntegrationTest
{
    protected readonly IUserClient UserClient;
    protected readonly UserEventConsumer UserEventConsumer;

    protected GenericIntegrationTest(DatabaseFixture db, KafkaFixture kafka)
    {
        var bootstrapServers = kafka.BootstrapServers;
        var factory = new WebApplicationFactory<MyProgram>()
            .WithWebHostBuilder(
                host =>
                {
                    host.UseSetting("ConnectionStrings:DefaultConnection", db.ConnectionString);
                    host.UseSetting("Kafka:BootstrapServers", bootstrapServers);
                }
            );
        UserClient = RestService.For<IUserClient>(factory.CreateClient());
        UserEventConsumer = new UserEventConsumer(bootstrapServers);
    }
}