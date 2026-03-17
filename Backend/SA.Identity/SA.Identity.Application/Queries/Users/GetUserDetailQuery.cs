using Mediator;
using SA.Identity.Application.Dtos.Users;

namespace SA.Identity.Application.Queries.Users;

public readonly record struct GetUserDetailQuery(
    Guid UserId) : IQuery<UserDetailDto>;
