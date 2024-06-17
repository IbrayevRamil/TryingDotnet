using TryingDotnet.DataAccess.Migrations;
using TryingDotnet.DI;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ScanIn(
            typeof(CreateUsersTable).Assembly,
            typeof(CreateTasksTable).Assembly,
            typeof(AddGuidColumn).Assembly
        ).For.Migrations()
    );
//builder.Services.AddLogging(lb => lb.AddFluentMigratorConsole());
builder.Services.RegisterDataAccess(builder.Configuration);
builder.Services.RegisterEvents();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();