using Mediator;
using SA.Identity.Application.Dtos.Users;
using SA.Identity.DataAccess.Interface.Repositories;
using Shared.Pagination;

namespace SA.Identity.Application.Queries.Users;

public sealed class ListUsersQueryHandler(
    IUserRepository userRepository) : IQueryHandler<ListUsersQuery, PagedResult<UserDto>>
{
    private const int MaxExportSize = 10_000;

    public async ValueTask<PagedResult<UserDto>> Handle(ListUsersQuery query, CancellationToken ct)
    {
        int pageSize;
        int skip;

        if (query.Export)
        {
            pageSize = MaxExportSize;
            skip = 0;
        }
        else
        {
            pageSize = Math.Clamp(query.PageSize, 1, 100);
            skip = (query.Page - 1) * pageSize;
        }

        var (items, total) = await userRepository.ListAsync(
            query.TenantId,
            skip,
            pageSize,
            query.Search,
            query.Status,
            query.RoleId,
            query.EntityId,
            query.SortBy,
            query.SortDir,
            ct);

        var dtos = items.Select(u => new UserDto(
            u.Id,
            u.Username,
            u.Email,
            u.FirstName,
            u.LastName,
            u.EntityId,
            u.EntityName,
            u.Status,
            u.LastLogin,
            u.Avatar,
            u.UserRoles.Select(ur => ur.Role.Name).ToList()))
            .ToList();

        return new PagedResult<UserDto>(dtos, total, query.Export ? 1 : query.Page, pageSize);
    }
}
