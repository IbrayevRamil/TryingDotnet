using TryingDotnet.DI;

namespace TryingDotnet.DataAccess.Outbox;

public class OutboxService(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var scopedProcessingService = scope.ServiceProvider
                .GetRequiredService<IScopedProcessingService>();

            await scopedProcessingService.Process(stoppingToken);
            await Task.Delay(TimeSpan.FromMilliseconds(50), stoppingToken);
        }
    }
}