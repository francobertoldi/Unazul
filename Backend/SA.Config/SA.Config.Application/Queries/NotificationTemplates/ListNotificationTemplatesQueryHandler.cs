using Mediator;
using SA.Config.Application.Dtos.NotificationTemplates;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Pagination;

namespace SA.Config.Application.Queries.NotificationTemplates;

public sealed class ListNotificationTemplatesQueryHandler(
    INotificationTemplateRepository templateRepository) : IQueryHandler<ListNotificationTemplatesQuery, PagedResult<NotificationTemplateDto>>
{
    public async ValueTask<PagedResult<NotificationTemplateDto>> Handle(ListNotificationTemplatesQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);
        var skip = (page - 1) * pageSize;

        var (items, total) = await templateRepository.ListAsync(skip, pageSize, query.Channel, query.Search, ct);

        var dtos = items
            .Select(t => new NotificationTemplateDto(
                t.Id,
                t.Code,
                t.Name,
                t.Channel,
                t.Status,
                t.CreatedAt))
            .ToList();

        return new PagedResult<NotificationTemplateDto>(dtos, total, page, pageSize);
    }
}
