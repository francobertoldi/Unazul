using MassTransit;

namespace Shared.Contract.Events;

[EntityName("NotificationTemplateCreated")]
public sealed record NotificationTemplateCreatedEvent(
    Guid TenantId,
    Guid TemplateId,
    string Code,
    string Channel,
    Guid CreatedBy,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
