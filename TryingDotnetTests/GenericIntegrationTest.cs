using Microsoft.AspNetCore.Mvc.Testing;
using Refit;
using TryingDotnet.Api;
using TryingDotnetTests.DataAccess;

namespace TryingDotnetTests;

[Collection(nameof(ContainersCollection))]
public class GenericIntegrationTest
{
    protected readonly IUserClient UserClient;

    protected GenericIntegrationTest(DatabaseFixture fixture)
    {
        var factory = new WebApplicationFactory<MyProgram>()
            .WithWebHostBuilder(
                host => host.UseSetting("ConnectionStrings:DefaultConnection", fixture.ConnectionString));
        UserClient = RestService.For<IUserClient>(factory.CreateClient());
    }
}