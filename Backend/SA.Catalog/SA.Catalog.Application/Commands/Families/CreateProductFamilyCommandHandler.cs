using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Families;

public sealed class CreateProductFamilyCommandHandler(
    IProductFamilyRepository familyRepository) : ICommandHandler<CreateProductFamilyCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProductFamilyCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Code))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        if (command.Code.Length > 15)
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        if (!ProductCategory.IsValidPrefix(command.Code))
            throw new ValidationException("CAT_INVALID_PREFIX", "Prefijo de codigo no reconocido. Prefijos validos: PREST, SEG, CTA, TARJETA, INV.");

        if (string.IsNullOrWhiteSpace(command.Description))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        var exists = await familyRepository.ExistsByCodeAsync(command.TenantId, command.Code, ct);
        if (exists)
            throw new ConflictException("CAT_DUPLICATE_FAMILY_CODE", "Ya existe una familia con ese codigo.");

        var family = ProductFamily.Create(command.TenantId, command.Code, command.Description, command.UserId);
        await familyRepository.AddAsync(family, ct);
        await familyRepository.SaveChangesAsync(ct);

        return family.Id;
    }
}
