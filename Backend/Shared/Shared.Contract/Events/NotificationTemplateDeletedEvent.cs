using MassTransit;

namespace Shared.Contract.Events;

[EntityName("NotificationTemplateDeleted")]
public sealed record NotificationTemplateDeletedEvent(
    Guid TenantId,
    Guid TemplateId,
    string Code,
    string Channel,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
