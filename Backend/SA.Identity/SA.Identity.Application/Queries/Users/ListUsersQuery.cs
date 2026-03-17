using Mediator;
using SA.Identity.Application.Dtos.Users;
using Shared.Contract.Enums;
using Shared.Pagination;

namespace SA.Identity.Application.Queries.Users;

public readonly record struct ListUsersQuery(
    Guid TenantId,
    int Page,
    int PageSize,
    string? Search,
    UserStatus? Status,
    Guid? RoleId,
    Guid? EntityId,
    string? SortBy,
    string? SortDir,
    bool Export) : IQuery<PagedResult<UserDto>>;
