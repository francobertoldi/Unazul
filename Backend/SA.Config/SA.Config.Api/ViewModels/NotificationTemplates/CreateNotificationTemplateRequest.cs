namespace SA.Config.Api.ViewModels.NotificationTemplates;

public sealed record CreateNotificationTemplateRequest(
    string Code,
    string Name,
    string Channel,
    string? Subject,
    string Body,
    string? Status);
