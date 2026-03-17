using MassTransit;

namespace Shared.Contract.Events;

[EntityName("ParameterUpdated")]
public sealed record ParameterUpdatedEvent(
    Guid TenantId,
    Guid ParameterId,
    string ParameterCode,
    string? OldValue,
    string? NewValue,
    Guid UpdatedBy,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
