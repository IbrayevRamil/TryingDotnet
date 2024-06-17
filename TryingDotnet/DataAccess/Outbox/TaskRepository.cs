using System.Data;
using System.Data.Common;
using System.Transactions;
using Dapper;

namespace TryingDotnet.DataAccess.Outbox;

public interface ITaskRepository
{
    Task<bool> AddTask(string topic, string payload, Guid correlationId, DateTimeOffset scheduledAt);
    Task<ScheduledTask?> PickTask();
    Task<bool> CancelTask(long id);
}

public class TaskRepository(DbConnection db) : ITaskRepository
{
    public async Task<bool> AddTask(string topic, string payload, Guid correlationId, DateTimeOffset scheduledAt)
    {
        var inserted = await db.ExecuteAsync(@"
                INSERT INTO tasks (topic, correlation_id, payload, scheduled_at) 
                VALUES (@topic, @correlationId, @payload, @scheduledAt) 
                ON CONFLICT DO NOTHING;
            ",
            new { topic, payload, correlationId, scheduledAt = scheduledAt.ToUniversalTime() }
        );
        return inserted > 0;
    }

    public async Task<ScheduledTask?> PickTask()
    {
        return await db.QuerySingleOrDefaultAsync<ScheduledTask>(@"
            WITH pick_task_cte AS (
                SELECT id 
                FROM tasks
                WHERE scheduled_at <= now()
                LIMIT 1
            )
            UPDATE tasks 
            SET scheduled_at = scheduled_at + INTERVAL '30 seconds'
            FROM pick_task_cte
            WHERE tasks.id = pick_task_cte.id
            RETURNING tasks.id, topic, payload, scheduled_at, correlation_id
        ");
    }

    public async Task<bool> CancelTask(long id)
    {
        var canceled = await db.ExecuteAsync(@"
            DELETE FROM tasks WHERE id = @id
        ", new { id });
        return canceled > 0;
    }
}