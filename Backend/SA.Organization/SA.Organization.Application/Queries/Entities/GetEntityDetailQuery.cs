using Mediator;
using SA.Organization.Application.Dtos.Entities;

namespace SA.Organization.Application.Queries.Entities;

public readonly record struct GetEntityDetailQuery(Guid Id) : IQuery<EntityDetailDto>;
