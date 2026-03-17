namespace SA.Catalog.Application.Interfaces;

public interface IEntityValidationService
{
    Task<bool> ValidateEntityExistsAsync(Guid tenantId, Guid entityId, CancellationToken ct = default);
}
