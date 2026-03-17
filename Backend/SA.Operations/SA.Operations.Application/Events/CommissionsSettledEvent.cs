namespace SA.Operations.Application.Events;

public sealed record CommissionsSettledEvent(
    Guid SettlementId,
    Guid TenantId,
    int ItemsCount,
    DateTimeOffset OccurredAt,
    Guid CorrelationId);
