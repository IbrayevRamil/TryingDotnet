using TryingDotnet.Api;
using TryingDotnet.Controllers;
using TryingDotnetTests.DataAccess;
using TryingDotnetTests.Events;
using TryingDotnetTests.Utils;

namespace TryingDotnetTests;

public class UserControllerTests(DatabaseFixture db, KafkaFixture kafka) : GenericIntegrationTest(db, kafka)
{
    
    [Fact]
    public async Task Should_Successfully_Insert_User()
    {
        var username = DataGenerationUtils.GenerateRandomString();
        var request = new AddUserRequest(username);
        var response = await UserClient.Add(request);
        
        Assert.True(response.IsSuccess);
        var expectedUser = response.Value with { Username = username };
        var actualUser = response.Value;
        Assert.Equal(expected: expectedUser, actual: actualUser);

        var getUserResponse = await UserClient.Get(response.Value.UserId);
        Assert.True(getUserResponse.IsSuccess);
        Assert.Equal(expected: expectedUser, actual: getUserResponse.Value);

        var producedMessage = UserEventConsumer.Consume(TimeSpan.FromSeconds(5));
        Assert.Equal(expected: expectedUser, actual: producedMessage);
    }
    
    [Fact]
    public async Task Should_Return_Error_When_User_Already_Exists()
    {
        var username = DataGenerationUtils.GenerateRandomString();
        var request = new AddUserRequest(username);
        var initialResponse = await UserClient.Add(request);
        var expectedUser = initialResponse.Value with { Username = username };
        var actualUser = initialResponse.Value;
        Assert.Equal(expected: expectedUser, actual: actualUser);
        
        var response = await UserClient.Add(request);
        Assert.False(response.IsSuccess);
        Assert.Equal(expected: RpcError.GeneralError, actual: response.Error);
    }
    
    [Fact]
    public async Task Should_Successfully_Get_Existing_User()
    {
        var username = DataGenerationUtils.GenerateRandomString();
        var request = new AddUserRequest(username);
        var response = await UserClient.Add(request);
        
        Assert.True(response.IsSuccess);

        var getUserResponse = await UserClient.Get(response.Value.UserId);
        Assert.True(getUserResponse.IsSuccess);
        Assert.Equal(expected: username, actual: getUserResponse.Value.Username);
    }
    
    [Fact]
    public async Task Should_Return_Not_Found_When_User_Doesnt_Exist()
    {
        var userId = Guid.NewGuid();
        var getUserResponse = await UserClient.Get(userId);
        Assert.False(getUserResponse.IsSuccess);
        Assert.Equal(expected: RpcError.NotFound, actual: getUserResponse.Error);
    }
}