using MassTransit;

namespace Shared.Contract.Events;

[EntityName("DomainEvent")]
public sealed record DomainEvent(
    Guid TenantId,
    Guid UserId,
    string UserName,
    string Operation,
    string Module,
    string Action,
    string? Detail,
    string? IpAddress,
    string? EntityType,
    Guid? EntityId,
    string? ChangesJson,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
