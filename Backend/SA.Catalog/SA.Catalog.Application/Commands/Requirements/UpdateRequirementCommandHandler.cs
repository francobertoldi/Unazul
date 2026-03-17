using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Requirements;

public sealed class UpdateRequirementCommandHandler(
    IProductRepository productRepository,
    IProductRequirementRepository requirementRepository) : ICommandHandler<UpdateRequirementCommand>
{
    public async ValueTask<Unit> Handle(UpdateRequirementCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Type))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        if (!ProductRequirement.ValidTypes.Contains(command.Type))
            throw new ValidationException("CAT_INVALID_REQUIREMENT_TYPE", "Tipo de requisito no valido.");

        var product = await productRepository.GetByIdAsync(command.ProductId, ct)
            ?? throw new NotFoundException("CAT_PRODUCT_NOT_FOUND", "Producto no encontrado.");

        if (product.Status == ProductStatus.Deprecated)
            throw new ConflictException("CAT_PRODUCT_DEPRECATED", "El producto esta deprecado y no se puede modificar.");

        var requirement = await requirementRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_REQUIREMENT_NOT_FOUND", "Requisito no encontrado.");

        requirement.Update(command.Name, command.Type, command.IsMandatory, command.Description);
        requirementRepository.Update(requirement);
        await requirementRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
