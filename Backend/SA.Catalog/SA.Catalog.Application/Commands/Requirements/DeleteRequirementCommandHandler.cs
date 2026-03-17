using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Requirements;

public sealed class DeleteRequirementCommandHandler(
    IProductRepository productRepository,
    IProductRequirementRepository requirementRepository) : ICommandHandler<DeleteRequirementCommand>
{
    public async ValueTask<Unit> Handle(DeleteRequirementCommand command, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, ct)
            ?? throw new NotFoundException("CAT_PRODUCT_NOT_FOUND", "Producto no encontrado.");

        if (product.Status == ProductStatus.Deprecated)
            throw new ConflictException("CAT_PRODUCT_DEPRECATED", "El producto esta deprecado y no se puede modificar.");

        var requirement = await requirementRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_REQUIREMENT_NOT_FOUND", "Requisito no encontrado.");

        requirementRepository.Delete(requirement);
        await requirementRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
