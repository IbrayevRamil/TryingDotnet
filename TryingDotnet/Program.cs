using System.Text.Json.Serialization;
using FluentMigrator.Runner;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TryingDotnet.DataAccess.Migrations;
using TryingDotnet.DI;
using TryingDotnet.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
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
builder.Services
    .AddOpenTelemetry()
    .UseOtlpExporter(protocol: OtlpExportProtocol.Grpc, baseUrl: new Uri("http://localhost:4317"))
    .ConfigureResource(resource => resource.AddService("TryingDotnet"))
    .WithMetrics(metrics =>
        metrics
            //.AddConsoleExporter()
            .AddAspNetCoreInstrumentation()
            .AddMeter("Npgsql")
    )
    .WithTracing(tracing =>
        tracing
            //.AddConsoleExporter()
            .AddAspNetCoreInstrumentation()
            .AddNpgsql()
    )
    .WithLogging(_ => {});
builder.Services.RegisterDataAccess(builder.Configuration);
builder.Services.RegisterEvents();
await builder.Services.RegisterTopics(builder.Configuration);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class MyProgram;