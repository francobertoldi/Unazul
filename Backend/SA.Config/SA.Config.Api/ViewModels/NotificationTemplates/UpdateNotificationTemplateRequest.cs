namespace SA.Config.Api.ViewModels.NotificationTemplates;

public sealed record UpdateNotificationTemplateRequest(
    string Name,
    string? Subject,
    string Body,
    string? Status);
