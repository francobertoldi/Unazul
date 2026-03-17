using MassTransit;

namespace Shared.Contract.Events;

[EntityName("MessageSent")]
public sealed record MessageSentEvent(
    Guid TenantId,
    Guid ApplicationId,
    string Channel,
    string Recipient,
    string? TemplateTitle,
    string? Subject,
    string Body,
    Guid SentBy,
    string SentByName,
    DateTimeOffset OccurredAt,
    Guid CorrelationId
);
