using SA.Catalog.Application.Interfaces;

namespace SA.Catalog.Infrastructure.Services;

public class EntityValidationService : IEntityValidationService
{
    public Task<bool> ValidateEntityExistsAsync(Guid tenantId, Guid entityId, CancellationToken ct = default)
    {
        // SECURITY: This service MUST be implemented before production deployment.
        // It should call Organization Service GET /api/v1/entities/{entityId}
        // to verify the entity exists and belongs to the given tenant.
        // Without this validation, any authenticated user can reference entities
        // from other tenants, breaking multi-tenant isolation.
        throw new NotImplementedException(
            "EntityValidationService is not implemented. " +
            "Configure IHttpClientFactory for Organization Service before deploying to production.");
    }
}
