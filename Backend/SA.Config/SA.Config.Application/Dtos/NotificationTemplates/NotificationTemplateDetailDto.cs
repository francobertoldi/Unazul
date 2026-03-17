namespace SA.Config.Application.Dtos.NotificationTemplates;

public sealed record NotificationTemplateDetailDto(
    Guid Id,
    string Code,
    string Name,
    string Channel,
    string? Subject,
    string Body,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy);
