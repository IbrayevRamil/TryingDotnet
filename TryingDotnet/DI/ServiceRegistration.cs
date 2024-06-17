using System.Data.Common;
using TryingDotnet.DataAccess;
using Npgsql;
using TryingDotnet.DataAccess.Outbox;
using TryingDotnet.DataAccess.Transaction;
using TryingDotnet.Events;
using TryingDotnet.Services;

namespace TryingDotnet.DI;

public static class ServiceRegistration
{
    public static void RegisterDataAccess(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddScoped<DbConnection>(_ =>
            new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IScopedProcessingService, OutboxScopedProcessingService>();
        services.AddHostedService<OutboxService>();
    }

    public static void RegisterEvents(this IServiceCollection services)
    {
        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        services.AddHostedService<UserEventConsumer>();
    }
}