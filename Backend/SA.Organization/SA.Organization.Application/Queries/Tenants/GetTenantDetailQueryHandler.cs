using Mediator;
using SA.Organization.Application.Dtos.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Queries.Tenants;

public sealed class GetTenantDetailQueryHandler(
    ITenantRepository tenantRepository) : IQueryHandler<GetTenantDetailQuery, TenantDetailDto>
{
    public async ValueTask<TenantDetailDto> Handle(GetTenantDetailQuery query, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException("ORG_TENANT_NOT_FOUND", "Tenant no encontrado.");

        var entityCount = await tenantRepository.CountEntitiesAsync(query.Id, ct);

        // UserCount is a stub for now — will be resolved via cross-service call
        var userCount = 0;

        return new TenantDetailDto(
            tenant.Id,
            tenant.Name,
            tenant.Identifier,
            tenant.Status.ToString(),
            tenant.Address,
            tenant.City,
            tenant.Province,
            tenant.ZipCode,
            tenant.Country,
            tenant.Phone,
            tenant.Email,
            tenant.LogoUrl,
            entityCount,
            userCount,
            tenant.CreatedAt,
            tenant.UpdatedAt);
    }
}
