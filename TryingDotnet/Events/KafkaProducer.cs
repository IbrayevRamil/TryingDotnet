using Confluent.Kafka;

namespace TryingDotnet.Events;

public enum KafkaResult
{
    Done,
    Failed
}

public interface IKafkaProducer
{
    public Task<KafkaResult> Produce(string topic, string message);
}

public class KafkaProducer : IKafkaProducer
{
    private readonly ILogger<KafkaProducer> _logger;
    private readonly IProducer<Null, string> _producer;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task<KafkaResult> Produce(string topic, string message)
    {
        KafkaResult result;
        try
        {
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            result = KafkaResult.Done;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to produce kafka message: topic={}, error={}", topic, e.Message);
            result = KafkaResult.Failed;
        }

        return result;
    }
}