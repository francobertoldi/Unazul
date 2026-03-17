using MassTransit;

namespace Shared.Contract.Events;

[EntityName("NotificationTemplateUpdated")]
public sealed record NotificationTemplateUpdatedEvent(
    Guid TenantId,
    Guid TemplateId,
    string Code,
    string Channel,
    Guid UpdatedBy,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
