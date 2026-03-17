namespace SA.Config.Api.ViewModels.NotificationTemplates;

public sealed record NotificationTemplateResponse(
    Guid Id,
    string Code,
    string Name,
    string Channel,
    string Status,
    DateTime CreatedAt);
