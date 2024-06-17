using System.Data;
using LanguageExt;

namespace TryingDotnet.DataAccess.Transaction;

public static class UnitOfWorkExtensions
{
    public static async Task<Either<TLeft, TRight>> Execute<TLeft, TRight>(
        this IUnitOfWork unitOfWork,
        Func<CancellationToken, Task<Either<TLeft, TRight>>> action,
        CancellationToken cancellationToken = default,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
    )
    {
        await unitOfWork.Begin(isolationLevel, cancellationToken);

        try
        {
            var result = await action(cancellationToken);

            if (result.IsRight)
            {
                await unitOfWork.Commit(cancellationToken);
            }
            else
            {
                await unitOfWork.Rollback(cancellationToken);
            }

            return result;
        }
        catch (Exception exception)
        {
            await unitOfWork.Rollback(cancellationToken); // Ensure rollback on exception
            throw new InvalidOperationException(
                "Transaction rollback has failed in a catch handler",
                exception
            );
        }
    }
}