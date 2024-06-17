using LanguageExt;
using TryingDotnet.DI;
using TryingDotnet.Events;

namespace TryingDotnet.DataAccess.Outbox;

public class OutboxScopedProcessingService(
    ITaskRepository taskRepository,
    IKafkaProducer kafkaProducer,
    ILogger<OutboxScopedProcessingService> logger
) : IScopedProcessingService
{
    public async Task Process(CancellationToken stoppingToken)
    {
        var pickedTask = await taskRepository.PickTask();
        if (pickedTask is not null)
        {
            await Handle(pickedTask);
        }
    }

    private async Task Handle(ScheduledTask task)
    {
        var produced = await kafkaProducer.Produce(topic: task.Topic, message: task.Payload);
        switch (produced)
        {
            case KafkaResult.Done:
                var canceled = await taskRepository.CancelTask(task.Id);
                if (!canceled) logger.LogWarning("Failed to cancel task: id={}", task.Id);
                break;
            case KafkaResult.Failed:
                logger.LogWarning(
                    "Failed to produce kafka message: topic={}, correlation_id={}",
                    task.Topic,
                    task.CorrelationId
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}