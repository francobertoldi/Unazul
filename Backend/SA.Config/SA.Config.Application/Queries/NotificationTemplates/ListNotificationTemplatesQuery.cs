using Mediator;
using SA.Config.Application.Dtos.NotificationTemplates;
using Shared.Pagination;

namespace SA.Config.Application.Queries.NotificationTemplates;

public readonly record struct ListNotificationTemplatesQuery(
    int Page,
    int PageSize,
    string? Channel,
    string? Search) : IQuery<PagedResult<NotificationTemplateDto>>;
