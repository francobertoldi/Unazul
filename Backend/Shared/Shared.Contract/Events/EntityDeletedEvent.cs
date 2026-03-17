using MassTransit;

namespace Shared.Contract.Events;

[EntityName("EntityDeleted")]
public sealed record EntityDeletedEvent(
    Guid TenantId,
    Guid EntityId,
    string EntityName,
    Guid DeletedBy,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
