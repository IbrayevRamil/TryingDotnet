using Microsoft.AspNetCore.Mvc;
using TryingDotnet.Api;
using TryingDotnet.DataAccess.Repositories;
using TryingDotnet.Services;

namespace TryingDotnet.Controllers;

public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("/api/users/{guid:guid}")]
    public async Task<Result<RpcError, User>> Get(Guid guid)
    {
        return (await userService.GetUser(guid))
            .Match(
                Left: error =>
                {
                    var rpcError = error switch
                    {
                        UserError.Duplicate => RpcError.GeneralError,
                        UserError.NotFound => RpcError.NotFound,
                        UserError.GeneralError => RpcError.GeneralError,
                        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
                    };
                    return Result<RpcError, User>.Failure(rpcError);
                },
                Right: Result<RpcError, User>.Success
            );
    }

    [HttpPost("/api/users/")]
    public async Task<Result<RpcError, User>> Add([FromBody] AddUserRequest request)
    {
        return (await userService.AddUser(request.Username))
            .Match(
                Left: _ => Result<RpcError, User>.Failure(RpcError.GeneralError),
                Right: Result<RpcError, User>.Success
            );
    }
}