namespace TryingDotnet.DataAccess.Outbox;

public record ScheduledTask(int Id, string Topic, string Payload, DateTime ScheduledAt, Guid CorrelationId);