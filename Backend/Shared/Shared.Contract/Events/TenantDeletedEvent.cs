using MassTransit;

namespace Shared.Contract.Events;

[EntityName("TenantDeleted")]
public sealed record TenantDeletedEvent(
    Guid TenantId,
    string TenantName,
    Guid DeletedBy,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
