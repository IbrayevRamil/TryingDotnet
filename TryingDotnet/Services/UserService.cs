using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Transactions;
using LanguageExt;
using TryingDotnet.Controllers;
using TryingDotnet.DataAccess;
using TryingDotnet.DataAccess.Outbox;
using TryingDotnet.DataAccess.Transaction;
using TryingDotnet.Events;

namespace TryingDotnet.Services;

public interface IUserService
{
    Task<Either<UserError, User>> AddUser(User user);
    Task<User?> GetUser(int index);
}

public class UserService(
    IUserRepository userRepository,
    ITaskRepository taskRepository,
    IUnitOfWork unitOfWork
) : IUserService
{
    public async Task<Either<UserError, User>> AddUser(User user)
    {
        return await unitOfWork.Execute(_ =>
            userRepository.AddUser(user)
                .BindAsync(async addedUser =>
                    await taskRepository.AddTask(
                        topic: "user",
                        payload: JsonSerializer.Serialize(user),
                        correlationId: Guid.NewGuid(),
                        scheduledAt: DateTimeOffset.Now
                    )
                        ? Either<UserError, User>.Right(addedUser)
                        : Either<UserError, User>.Left(UserError.GeneralError)));
    }

    public Task<User?> GetUser(int index)
    {
        return userRepository.GetUser(index);
    }
}