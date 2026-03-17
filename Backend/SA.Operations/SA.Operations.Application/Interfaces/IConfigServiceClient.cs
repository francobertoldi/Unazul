namespace SA.Operations.Application.Interfaces;

public interface IConfigServiceClient
{
    Task<NotificationTemplateResult?> GetNotificationTemplateAsync(Guid templateId, CancellationToken ct = default);
}

public sealed record NotificationTemplateResult(Guid Id, string Title, string? Subject, string Content, string Channel);
