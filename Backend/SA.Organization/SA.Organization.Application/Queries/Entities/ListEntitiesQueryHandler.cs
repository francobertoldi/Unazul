using Mediator;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Pagination;

namespace SA.Organization.Application.Queries.Entities;

public sealed class ListEntitiesQueryHandler(
    IEntityRepository entityRepository) : IQueryHandler<ListEntitiesQuery, PagedResult<EntityDto>>
{
    public async ValueTask<PagedResult<EntityDto>> Handle(ListEntitiesQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (query.Page - 1) * pageSize;

        var (entities, total) = await entityRepository.ListAsync(
            skip, pageSize, query.Search, query.Status, query.Type, query.Sort, query.Order, ct);

        var items = entities
            .Select(e => new EntityDto(
                e.Id,
                e.Name,
                e.Cuit,
                e.Type.ToString(),
                e.Status.ToString(),
                e.City,
                e.Province,
                e.CreatedAt))
            .ToList();

        return new PagedResult<EntityDto>(items, total, query.Page, pageSize);
    }
}
