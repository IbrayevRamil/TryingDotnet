using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TryingDotnet.Controllers;
using TryingDotnetTests.DataAccess;

namespace TryingDotnetTests;

public class UserControllerTests(DatabaseFixture fixture) : GenericIntegrationTest(fixture)
{
    [Fact]
    public async Task Should_Successfully_Insert_User()
    {
        var user = new User(Guid.NewGuid().ToString());
        var response = await Client.PostAsJsonAsync("/api/users", user);
        var responseBody = await response.Content.ReadFromJsonAsync<Result<RpcError, User>>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.IsSuccess);
        Assert.Equal(expected: user, actual: responseBody.Value);
    }
    
    [Fact]
    public async Task Should_Return_Error_When_User_Already_Exists()
    {
        var user = new User(Guid.NewGuid().ToString());
        var initialResponse = await Client.PostAsJsonAsync("/api/users", user);
        var initialResponseBody = await initialResponse.Content.ReadFromJsonAsync<Result<RpcError, User>>();
        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);
        Assert.NotNull(initialResponseBody);
        Assert.True(initialResponseBody.IsSuccess);
        Assert.Equal(expected: user, actual: initialResponseBody.Value);
        
        var response = await Client.PostAsJsonAsync("/api/users", user);
        var responseBody = await response.Content.ReadFromJsonAsync<Result<RpcError, User>>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.False(responseBody.IsSuccess);
        Assert.Equal(expected: RpcError.GeneralError, actual: responseBody.Error);
    }
}