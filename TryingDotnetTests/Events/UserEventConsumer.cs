using System.Text.Json;
using Confluent.Kafka;
using TryingDotnet.Api;

namespace TryingDotnetTests.Events;

public class UserEventConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;

    public UserEventConsumer(string bootstrapServers)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "TryingDotnetConsumerGroup",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    public User Consume(TimeSpan timeout)
    {
        _consumer.Subscribe("user");

        try
        {
            var result = _consumer.Consume(timeout);
            if (result is null) Assert.Fail("Failed to consume message");

            var message = result.Message.Value;

            if (message is null) Assert.Fail("Failed to consume message");

            var user = JsonSerializer.Deserialize<User>(message);

            if (user is null) Assert.Fail("Failed to consume message");

            return user;
        }
        catch (Exception e)
        {
            Assert.Fail($"Failed to consume message: {e.Message}");
            throw;
        }
    }
}