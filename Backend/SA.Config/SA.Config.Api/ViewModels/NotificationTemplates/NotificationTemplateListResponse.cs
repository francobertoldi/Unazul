namespace SA.Config.Api.ViewModels.NotificationTemplates;

public sealed record NotificationTemplateListResponse(
    IReadOnlyList<NotificationTemplateResponse> Items,
    int Total,
    int Page,
    int PageSize);
