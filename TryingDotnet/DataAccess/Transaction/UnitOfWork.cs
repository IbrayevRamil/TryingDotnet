using System.Data;
using System.Data.Common;

namespace TryingDotnet.DataAccess.Transaction;

public interface IUnitOfWork
{
    Task Begin(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    Task Commit(CancellationToken cancellationToken = default);
    Task Rollback(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWork(DbConnection connection) : IUnitOfWork, IAsyncDisposable
{
    private DbTransaction? _transaction;

    private bool _isRollbacked;
    private bool _isTerminated;
    private int _transactionCounter;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Task Begin(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        return CallSafe(async cToken =>
        {
            if (_isTerminated)
            {
                throw new InvalidOperationException("Unit of work is in terminal state");
            }

            if (_isRollbacked)
            {
                throw new InvalidOperationException("Unit of work has already been rolledback");
            }

            if (_transaction == null)
            {
                await connection.OpenAsync(cancellationToken);
                _transaction = await connection.BeginTransactionAsync(isolationLevel, cToken)
                    .ConfigureAwait(false);
            }
            else
            {
                if (_transaction.IsolationLevel != isolationLevel)
                {
                    throw new InvalidOperationException("Transaction with different isolation level already exists");
                }
            }

            _transactionCounter++;
        }, cancellationToken);
    }

    public Task Commit(CancellationToken cancellationToken)
    {
        return CallSafe(CommitInternal, cancellationToken);
    }

    public Task Rollback(CancellationToken cancellationToken)
    {
        return CallSafe(RollbackInternal, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _semaphore.WaitAsync();
        }
        catch (NullReferenceException)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }
        catch (ObjectDisposedException)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        try
        {
            if (_transaction == null)
            {
                return;
            }
            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                await connection.DisposeAsync();
                _transaction = null;
            }
        }
        finally
        {
            _semaphore.Dispose();
        }
    }

    private async Task CallSafe(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
        }
        catch (NullReferenceException)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }
        catch (ObjectDisposedException)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        try
        {
            await action(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task CommitInternal(CancellationToken cancellationToken)
    {
        if (_isTerminated)
        {
            throw new InvalidOperationException("Unit of work is in terminal state");
        }

        if (_isRollbacked)
        {
            throw new InvalidOperationException("Unit of work has already been rolledback");
        }

        if (_transaction == null)
        {
            throw new InvalidOperationException("Nothing to commit");
        }
        _transactionCounter--;

        if (_transactionCounter != 0) return;
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        await connection.CloseAsync();
        _transaction = null;
        _isTerminated = true;
    }

    private async Task RollbackInternal(CancellationToken cancellationToken)
    {
        if (_isTerminated)
        {
            throw new InvalidOperationException("Unit of work is in terminal state");
        }

        if (!_isRollbacked)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Nothing to rollback");
            }

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            await connection.CloseAsync();
            await connection.DisposeAsync();
            _transaction = null;
            _isRollbacked = true;
        }

        _transactionCounter--;
        if (_transactionCounter == 0)
        {
            _isTerminated = true;
        }
    }
}