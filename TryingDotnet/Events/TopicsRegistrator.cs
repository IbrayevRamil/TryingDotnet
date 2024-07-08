using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace TryingDotnet.Events;

public static class TopicsRegistrator
{
    public static async Task RegisterTopics(this IServiceCollection services, ConfigurationManager configuration)
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };
        var topics = ((List<IConfigurationSection>)configuration.GetSection("Kafka:Topics").GetChildren())
            .Select(it => it.Key);
        var topicSpecifications = topics.Select(topic =>
            new TopicSpecification { Name = topic, ReplicationFactor = 1, NumPartitions = 1 }
        );

        using var adminClient = new AdminClientBuilder(config).Build();

        try
        {
            await adminClient.CreateTopicsAsync(topicSpecifications);
        }
        catch (CreateTopicsException e)
        {
            if (e.Results[0]?.Error.Code != ErrorCode.TopicAlreadyExists)
            {
                throw;
            }
        }
    }
}