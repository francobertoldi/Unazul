using Mediator;
using SA.Organization.Application.Dtos.Entities;
using Shared.Pagination;

namespace SA.Organization.Application.Queries.Entities;

public readonly record struct ListEntitiesQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    string? Type,
    string? Sort,
    string Order) : IQuery<PagedResult<EntityDto>>;
