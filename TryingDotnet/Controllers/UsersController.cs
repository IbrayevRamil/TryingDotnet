using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using TryingDotnet.Services;

namespace TryingDotnet.Controllers;

public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("/api/users/{index}")]
    public async Task<Result<RpcError, User>> Get(int index)
    {
        var user = await userService.GetUser(index);
        return user is not null
            ? Result<RpcError, User>.Success<RpcError, User>(user)
            : Result<RpcError, User>.Failure<RpcError, User>(RpcError.NotFound);
    }

    [HttpPost("/api/users/")]
    public async Task<Result<RpcError, User>> Add([FromBody] User user)
    {
        return (await userService.AddUser(user))
            .Match(
                Left: _ => Result<RpcError, User>.Failure<RpcError, User>(RpcError.GeneralError),
                Right: Result<RpcError, User>.Success<RpcError, User>
            );
    }
}

public record User(string Username);