using MassTransit;

namespace Shared.Contract.Events;

[EntityName("BranchDeleted")]
public sealed record BranchDeletedEvent(
    Guid TenantId,
    Guid EntityId,
    Guid BranchId,
    string BranchName,
    Guid DeletedBy,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
