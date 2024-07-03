using Refit;
using TryingDotnet.Controllers;

namespace TryingDotnet.Api;

public interface IUserClient
{
    [Get("/api/users/{guid}")]
    public Task<Result<RpcError, User>> Get(Guid guid);

    [Post("/api/users/")]
    public Task<Result<RpcError, User>> Add([Body] AddUserRequest request);
}