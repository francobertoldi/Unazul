using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Families;

public sealed class UpdateProductFamilyCommandHandler(
    IProductFamilyRepository familyRepository) : ICommandHandler<UpdateProductFamilyCommand>
{
    public async ValueTask<Unit> Handle(UpdateProductFamilyCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Description))
            throw new ValidationException("CAT_MISSING_REQUIRED_FIELDS", "Campos requeridos faltantes.");

        var family = await familyRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_FAMILY_NOT_FOUND", "Familia no encontrada.");

        family.Update(command.Description, command.UserId);
        familyRepository.Update(family);
        await familyRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
