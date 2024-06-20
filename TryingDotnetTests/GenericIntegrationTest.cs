using Microsoft.AspNetCore.Mvc.Testing;
using TryingDotnetTests.DataAccess;

namespace TryingDotnetTests;

[Collection(nameof(ContainersCollection))]
public class GenericIntegrationTest
{
    protected readonly HttpClient Client;
    protected readonly IServiceProvider ServiceProvider;

    protected GenericIntegrationTest(DatabaseFixture fixture)
    {
        var factory = new WebApplicationFactory<MyProgram>()
            .WithWebHostBuilder(
                host => host.UseSetting("ConnectionStrings:DefaultConnection", fixture.ConnectionString));
        ServiceProvider = factory.Services;
        Client = factory.CreateClient();
    }
}