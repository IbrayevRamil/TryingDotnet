using FluentMigrator;

namespace TryingDotnet.DataAccess.Migrations;

[Migration(2024060301)]
public class CreateTasksTable : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE tasks (
                id SERIAL PRIMARY KEY,
                topic VARCHAR(64) NOT NULL,
                payload TEXT NOT NULL,
                scheduled_at TIMESTAMPTZ NOT NULL,
                correlation_id uuid NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT now()
            );
        ");
        Execute.Sql(@"
            CREATE INDEX idx_task ON tasks(scheduled_at, id DESC);
        ");
        Execute.Sql(@"
            CREATE UNIQUE INDEX idx_correlation_id ON tasks(topic, correlation_id);
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE tasks;");
    }
}