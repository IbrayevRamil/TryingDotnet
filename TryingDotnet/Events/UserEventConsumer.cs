using Confluent.Kafka;

namespace TryingDotnet.Events;

public class UserEventConsumer : BackgroundService
{
    private readonly ILogger<UserEventConsumer> _logger;
    private readonly IConsumer<Ignore, string> _consumer;

    public UserEventConsumer(ILogger<UserEventConsumer> logger, IConfiguration configuration)
    {
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "UserConsumerGroup",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    protected override Task ExecuteAsync(CancellationToken token)
    {
        return Task.Run(() => Consume(token), token);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Consume(CancellationToken token)
    {
        _consumer.Subscribe("user");

        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(token);
                if (result is null) return;

                var message = result.Message.Value;
                _logger.LogInformation("Successfully consumed: {}", message);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Failed to consume kafka message: {}", e.Message);
            }
        }
    }
}