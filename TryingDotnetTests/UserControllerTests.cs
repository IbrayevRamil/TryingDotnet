using System.Net;
using System.Net.Http.Json;
using LanguageExt.UnsafeValueAccess;
using Microsoft.Extensions.DependencyInjection;
using TryingDotnet.Api;
using TryingDotnet.Controllers;
using TryingDotnet.DataAccess.Repositories;
using TryingDotnetTests.DataAccess;
using TryingDotnetTests.Utils;

namespace TryingDotnetTests;

public class UserControllerTests(DatabaseFixture fixture) : GenericIntegrationTest(fixture)
{
    [Fact]
    public async Task Should_Successfully_Insert_User()
    {
        var username = DataGenerationUtils.GenerateRandomString();
        var request = new AddUserRequest(username);
        var response = await Client.PostAsJsonAsync("/api/users", request);
        var responseBody = await response.Content.ReadFromJsonAsync<Result<RpcError, User>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.IsSuccess);
        var actualUser = responseBody.Value;
        Assert.Equal(expected: username, actual: actualUser.Username);

        using (var scope = ServiceProvider.CreateScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var actualDbUser = await userRepository.GetUser(actualUser.UserId);
            Assert.True(actualDbUser.IsRight);
            var expectedDbUser = actualUser with { Username = username };
            Assert.Equal(expected: expectedDbUser, actual: actualDbUser.ValueUnsafe());
        }
    }
    
    [Fact]
    public async Task Should_Return_Error_When_User_Already_Exists()
    {
        var username = DataGenerationUtils.GenerateRandomString();
        var request = new AddUserRequest(username);
        var initialResponse = await Client.PostAsJsonAsync("/api/users", request);
        var initialResponseBody = await initialResponse.Content.ReadFromJsonAsync<Result<RpcError, User>>();
        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);
        Assert.NotNull(initialResponseBody);
        var actualUser = initialResponseBody.Value;
        Assert.Equal(expected: username, actual: actualUser.Username);
        
        var response = await Client.PostAsJsonAsync("/api/users", request);
        var responseBody = await response.Content.ReadFromJsonAsync<Result<RpcError, User>>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.False(responseBody.IsSuccess);
        Assert.Equal(expected: RpcError.GeneralError, actual: responseBody.Error);
    }
    
    [Fact]
    public async Task Should_Successfully_Get_Existing_User()
    {
        var username = DataGenerationUtils.GenerateRandomString();
        var request = new AddUserRequest(username);
        var response = await Client.PostAsJsonAsync("/api/users", request);
        var responseBody = await response.Content.ReadFromJsonAsync<Result<RpcError, User>>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.IsSuccess);

        var getUserResponse = await Client.GetFromJsonAsync<Result<RpcError, User>>($"/api/users/{responseBody.Value.UserId}");
        Assert.NotNull(getUserResponse);
        Assert.True(getUserResponse.IsSuccess);
        Assert.Equal(expected: username, actual: getUserResponse.Value.Username);
    }
    
    [Fact]
    public async Task Should_Return_Not_Found_When_User_Doesnt_Exist()
    {
        var userId = Guid.NewGuid();
        var getUserResponse = await Client.GetFromJsonAsync<Result<RpcError, User>>($"/api/users/{userId}");
        Assert.NotNull(getUserResponse);
        Assert.False(getUserResponse.IsSuccess);
        Assert.Equal(expected: RpcError.NotFound, actual: getUserResponse.Error);
    }
}