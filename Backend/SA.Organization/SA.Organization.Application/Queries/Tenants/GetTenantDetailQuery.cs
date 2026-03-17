using Mediator;
using SA.Organization.Application.Dtos.Tenants;

namespace SA.Organization.Application.Queries.Tenants;

public readonly record struct GetTenantDetailQuery(Guid Id) : IQuery<TenantDetailDto>;
