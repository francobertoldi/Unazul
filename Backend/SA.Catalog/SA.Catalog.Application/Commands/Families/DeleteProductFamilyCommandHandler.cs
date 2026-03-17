using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Families;

public sealed class DeleteProductFamilyCommandHandler(
    IProductFamilyRepository familyRepository) : ICommandHandler<DeleteProductFamilyCommand>
{
    public async ValueTask<Unit> Handle(DeleteProductFamilyCommand command, CancellationToken ct)
    {
        var family = await familyRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_FAMILY_NOT_FOUND", "Familia no encontrada.");

        var productCount = await familyRepository.CountProductsAsync(command.Id, ct);
        if (productCount > 0)
            throw new ConflictException("CAT_FAMILY_HAS_PRODUCTS", "Familia con productos asociados, no se puede eliminar.");

        familyRepository.Delete(family);
        await familyRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
