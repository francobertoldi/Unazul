using Mediator;
using SA.Catalog.Application.Interfaces;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Products;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IProductFamilyRepository familyRepository,
    IEntityValidationService entityValidationService) : ICommandHandler<CreateProductCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProductCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Code))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        var family = await familyRepository.GetByIdAsync(command.FamilyId, ct)
            ?? throw new NotFoundException("CAT_FAMILY_NOT_FOUND", "Familia no encontrada.");

        bool entityExists;
        try
        {
            entityExists = await entityValidationService.ValidateEntityExistsAsync(
                command.TenantId, command.EntityId, ct);
        }
        catch (HttpRequestException)
        {
            throw new ValidationException("CAT_ENTITY_SERVICE_UNAVAILABLE", "El servicio de entidades no esta disponible.");
        }

        if (!entityExists)
            throw new NotFoundException("CAT_ENTITY_NOT_FOUND", "Entidad no encontrada.");

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

        var product = Product.Create(
            command.TenantId, command.EntityId, command.FamilyId,
            command.Name, command.Code, command.Description,
            status, validFrom, validTo, command.UserId);

        await productRepository.AddAsync(product, ct);
        await productRepository.SaveChangesAsync(ct);

        return product.Id;
    }
}
