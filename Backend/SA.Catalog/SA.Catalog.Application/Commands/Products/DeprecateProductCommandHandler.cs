using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Products;

public sealed class DeprecateProductCommandHandler(
    IProductRepository productRepository) : ICommandHandler<DeprecateProductCommand>
{
    public async ValueTask<Unit> Handle(DeprecateProductCommand command, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_PRODUCT_NOT_FOUND", "Producto no encontrado.");

        if (product.Status == ProductStatus.Deprecated)
            throw new ConflictException("CAT_PRODUCT_DEPRECATED", "El producto ya esta deprecado.");

        product.Deprecate(command.UserId);
        productRepository.Update(product);
        await productRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
