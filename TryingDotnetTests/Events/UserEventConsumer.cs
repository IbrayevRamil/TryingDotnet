using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using TryingDotnet.Api;

namespace TryingDotnetTests.Events;

//TODO ADD KAFKA TEST CONTAINER AND TESTS FOR PRODUCERS OF UserEvents
public class UserEventConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ConcurrentQueue<User> _events = new();

    public UserEventConsumer(IConfiguration configuration)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "UserConsumerGroup",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    public void Consume(CancellationToken token)
    {
        _consumer.Subscribe("user");

        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(token);
                if (result is null) Assert.Fail("Failed to consume message");

                var message = result.Message.Value;
                
                if (message is null) Assert.Fail("Failed to consume message");

                var user = JsonSerializer.Deserialize<User>(message);

                if (user is null) Assert.Fail("Failed to consume message");
                
                _events.Enqueue(user);
            }
            catch (Exception e)
            {
                Assert.Fail($"Failed to consume message: {e.Message}");
            }
        }
    }
}