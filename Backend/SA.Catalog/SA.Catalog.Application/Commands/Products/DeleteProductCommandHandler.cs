using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Products;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IProductRequirementRepository requirementRepository) : ICommandHandler<DeleteProductCommand>
{
    public async ValueTask<Unit> Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_PRODUCT_NOT_FOUND", "Producto no encontrado.");

        var hasPlans = await productRepository.HasPlansAsync(command.Id, ct);
        if (hasPlans)
            throw new ConflictException("CAT_PRODUCT_HAS_PLANS", "El producto tiene planes asociados y no se puede eliminar.");

        await requirementRepository.DeleteByProductIdAsync(command.Id, ct);
        productRepository.Delete(product);
        await productRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
