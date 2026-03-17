using MassTransit;

namespace Shared.Contract.Events;

[EntityName("RoleUpdated")]
public sealed record RoleUpdatedEvent(
    Guid TenantId,
    Guid RoleId,
    string RoleName,
    Guid[] AddedPermissionIds,
    Guid[] RemovedPermissionIds,
    Guid UpdatedBy,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
