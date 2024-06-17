using System.Data;
using System.Data.Common;
using Dapper;
using LanguageExt;
using LanguageExt.Common;
using Npgsql;
using TryingDotnet.Controllers;

namespace TryingDotnet.DataAccess;

public interface IUserRepository
{
    Task<Either<UserError, User>> AddUser(User user);
    Task<User?> GetUser(int index);
}

public class UserRepository(DbConnection db) : IUserRepository
{
    public async Task<Either<UserError, User>> AddUser(User user)
    {
        var inserted = await db.ExecuteAsync(@"
                INSERT INTO users (username) VALUES (@Username) ON CONFLICT DO NOTHING;
            ", user);
        return inserted > 0
            ? Either<UserError, User>.Right(user)
            : Either<UserError, User>.Left(UserError.GeneralError);
    }

    public Task<User?> GetUser(int id)
    {
        return db.QuerySingleOrDefaultAsync<User>(@"
                SELECT username FROM users WHERE id = @id
            ", new { id });
    }
}

public enum UserError
{
    Duplicate,
    GeneralError
}

public static class As
{
    public static async Task<Either<K, U>> Map2<T, U, K>(
        this Task<T> self,
        Func<T, U> map,
        Func<Exception, K> ex
    )
    {
        Func<T, U> func = map;
        try
        {
            var self1 = await self;
            return Either<K, U>.Right(func(self1));
        }
        catch (Exception e)
        {
            return Either<K, U>.Left(ex(e));
        }
    }
}