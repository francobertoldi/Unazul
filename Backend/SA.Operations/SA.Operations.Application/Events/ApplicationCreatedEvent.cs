namespace SA.Operations.Application.Events;

public sealed record ApplicationCreatedEvent(
    Guid ApplicationId,
    string Code,
    Guid TenantId,
    Guid EntityId,
    Guid ProductId,
    DateTimeOffset OccurredAt,
    Guid CorrelationId);
