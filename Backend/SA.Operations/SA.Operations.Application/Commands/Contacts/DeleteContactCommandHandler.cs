using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Contacts;

public sealed class DeleteContactCommandHandler(
    IContactRepository contactRepository) : ICommandHandler<DeleteContactCommand, DeleteContactResult>
{
    public async ValueTask<DeleteContactResult> Handle(DeleteContactCommand command, CancellationToken ct)
    {
        var contact = await contactRepository.GetByIdAsync(command.ContactId, ct);
        if (contact is null || contact.TenantId != command.TenantId)
            throw new NotFoundException("OPS_CONTACT_NOT_FOUND", "Contacto no encontrado.");

        contactRepository.Delete(contact);
        await contactRepository.SaveChangesAsync(ct);

        return new DeleteContactResult(contact.Id);
    }
}
