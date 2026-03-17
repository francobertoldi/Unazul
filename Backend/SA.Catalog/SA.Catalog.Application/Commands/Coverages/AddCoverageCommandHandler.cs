using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Coverages;

public sealed class AddCoverageCommandHandler(
    IProductPlanRepository planRepository,
    IProductRepository productRepository,
    IProductFamilyRepository familyRepository,
    ICoverageRepository coverageRepository) : ICommandHandler<AddCoverageCommand, Guid>
{
    public async ValueTask<Guid> Handle(AddCoverageCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.CoverageType))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        if (command.SumInsured <= 0)
            throw new ValidationException("CAT_INVALID_SUM_INSURED", "La suma asegurada debe ser mayor a cero.");

        var plan = await planRepository.GetByIdAsync(command.PlanId, ct)
            ?? throw new NotFoundException("CAT_PLAN_NOT_FOUND", "Plan no encontrado.");

        var product = await productRepository.GetByIdAsync(plan.ProductId, ct)
            ?? throw new NotFoundException("CAT_PRODUCT_NOT_FOUND", "Producto no encontrado.");

        var family = await familyRepository.GetByIdAsync(product.FamilyId, ct)
            ?? throw new NotFoundException("CAT_FAMILY_NOT_FOUND", "Familia no encontrada.");

        var category = ProductCategory.GetCategoryFromCode(family.Code);
        if (category != ProductCategory.Insurance)
            throw new ValidationException("CAT_COVERAGE_NOT_INSURANCE", "Las coberturas solo aplican a productos de seguro.");

        var duplicateName = await coverageRepository.ExistsByNameAsync(command.PlanId, command.Name, ct);
        if (duplicateName)
            throw new ConflictException("CAT_DUPLICATE_COVERAGE_NAME", "Ya existe una cobertura con ese nombre en el plan.");

        var coverage = Coverage.Create(
            command.PlanId, command.TenantId,
            command.Name, command.CoverageType, command.SumInsured,
            command.Premium, command.GracePeriodDays);

        await coverageRepository.AddAsync(coverage, ct);
        await coverageRepository.SaveChangesAsync(ct);

        return coverage.Id;
    }
}
