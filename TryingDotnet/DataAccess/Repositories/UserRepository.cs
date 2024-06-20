using System.Data.Common;
using Dapper;
using LanguageExt;
using TryingDotnet.Api;

namespace TryingDotnet.DataAccess.Repositories;

public interface IUserRepository
{
    Task<Either<UserError, User>> AddUser(Guid userId, string username);
    Task<Either<UserError, User>> GetUser(Guid guid);
}

public class UserRepository(DbConnection db) : IUserRepository
{
    public async Task<Either<UserError, User>> AddUser(Guid userId, string username)
    {
        var user = await db.QuerySingleOrDefaultAsync<User>(@"
                INSERT INTO users (username, user_id) VALUES (@username, @userId) ON CONFLICT DO NOTHING RETURNING user_id, username;
            ", new { username, userId });
        return user is not null
            ? Either<UserError, User>.Right(user)
            : Either<UserError, User>.Left(UserError.GeneralError);
    }

    public async Task<Either<UserError, User>> GetUser(Guid guid)
    {
        var user = await db.QuerySingleOrDefaultAsync<User>(@"
                SELECT user_id, username FROM users WHERE user_id = @guid
            ", new { guid });
        return user is not null
            ? Either<UserError, User>.Right(user)
            : Either<UserError, User>.Left(UserError.NotFound);
    }
}

public enum UserError
{
    Duplicate,
    NotFound,
    GeneralError
}