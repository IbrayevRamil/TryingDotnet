using FluentMigrator;

namespace TryingDotnet.DataAccess.Migrations;

[Migration(2024052701)]
public class CreateUsersTable : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(50) NOT NULL 
            );
        ");
        Execute.Sql(@"
            CREATE UNIQUE INDEX idx_users_username ON users(username);
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE users;");
    }
}

[Migration(2024060302)]
public class AddGuidColumn : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            ALTER TABLE users ADD COLUMN IF NOT EXISTS user_id uuid DEFAULT gen_random_uuid()
        ");
        Execute.Sql(@"
            CREATE UNIQUE INDEX idx_users_user_id ON users(user_id);
        ");
    }

    public override void Down()
    {
        Execute.Sql("ALTER TABLE users DROP COLUMN IF EXISTS user_id;");
    }
}