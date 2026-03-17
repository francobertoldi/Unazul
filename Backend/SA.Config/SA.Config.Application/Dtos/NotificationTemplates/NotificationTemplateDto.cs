namespace SA.Config.Application.Dtos.NotificationTemplates;

public sealed record NotificationTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string Channel,
    string Status,
    DateTime CreatedAt);
