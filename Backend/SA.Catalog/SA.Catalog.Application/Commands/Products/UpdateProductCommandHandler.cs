using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Products;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository) : ICommandHandler<UpdateProductCommand>
{
    public async ValueTask<Unit> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Code))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        var product = await productRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_PRODUCT_NOT_FOUND", "Producto no encontrado.");

        if (product.Status == ProductStatus.Deprecated)
            throw new ConflictException("CAT_PRODUCT_DEPRECATED", "El producto esta deprecado y no se puede modificar.");

        if (!Enum.TryParse<ProductStatus>(command.Status, true, out var status))
            throw new ValidationException("CAT_INVALID_STATUS", "Estado no valido.");

        if (status == ProductStatus.Deprecated)
            throw new ValidationException("CAT_INVALID_STATUS", "Estado no valido.");

        if (!DateOnly.TryParse(command.ValidFrom, out var validFrom))
            throw new ValidationException("CAT_INVALID_DATE_FORMAT", "Formato de fecha invalido.");

        DateOnly? validTo = null;
        if (!string.IsNullOrWhiteSpace(command.ValidTo))
        {
            if (!DateOnly.TryParse(command.ValidTo, out var parsedValidTo))
                throw new ValidationException("CAT_INVALID_DATE_FORMAT", "Formato de fecha invalido.");

            if (parsedValidTo < validFrom)
                throw new ValidationException("CAT_INVALID_DATE_RANGE", "La fecha de fin debe ser posterior a la fecha de inicio.");

            validTo = parsedValidTo;
        }

        product.Update(command.Name, command.Code, command.Description,
            status, validFrom, validTo, command.UserId);

        productRepository.Update(product);
        await productRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
