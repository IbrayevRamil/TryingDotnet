namespace TryingDotnet.DI;

public interface IScopedProcessingService
{
    Task Process(CancellationToken stoppingToken);
}