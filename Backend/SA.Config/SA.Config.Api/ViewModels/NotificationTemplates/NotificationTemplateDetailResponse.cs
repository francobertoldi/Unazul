namespace SA.Config.Api.ViewModels.NotificationTemplates;

public sealed record NotificationTemplateDetailResponse(
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
