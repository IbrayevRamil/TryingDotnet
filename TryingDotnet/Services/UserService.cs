using System.Text.Json;
using LanguageExt;
using TryingDotnet.Api;
using TryingDotnet.DataAccess.Outbox;
using TryingDotnet.DataAccess.Repositories;
using TryingDotnet.DataAccess.Transaction;

namespace TryingDotnet.Services;

public interface IUserService
{
    Task<Either<UserError, User>> AddUser(string username);
    Task<Either<UserError, User>> GetUser(Guid guid);
}

public class UserService(
    IUserRepository userRepository,
    ITaskRepository taskRepository,
    IUnitOfWork unitOfWork
) : IUserService
{
    public async Task<Either<UserError, User>> AddUser(string username)
    {
        var userId = Guid.NewGuid();
        return await unitOfWork.Execute(_ =>
            userRepository.AddUser(userId, username)
                .BindAsync(async addedUser =>
                    await taskRepository.AddTask(
                        topic: "user",
                        payload: JsonSerializer.Serialize(addedUser),
                        correlationId: addedUser.UserId,
                        scheduledAt: DateTimeOffset.Now
                    )
                        ? Either<UserError, User>.Right(addedUser)
                        : Either<UserError, User>.Left(UserError.GeneralError)));
    }

    public Task<Either<UserError, User>> GetUser(Guid guid)
    {
        return userRepository.GetUser(guid);
    }
}