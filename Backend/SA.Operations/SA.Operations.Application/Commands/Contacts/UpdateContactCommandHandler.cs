using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Contacts;

public sealed class UpdateContactCommandHandler(
    IContactRepository contactRepository) : ICommandHandler<UpdateContactCommand, UpdateContactResult>
{
    public async ValueTask<UpdateContactResult> Handle(UpdateContactCommand command, CancellationToken ct)
    {
        var contact = await contactRepository.GetByIdAsync(command.ContactId, ct);
        if (contact is null || contact.TenantId != command.TenantId)
            throw new NotFoundException("OPS_CONTACT_NOT_FOUND", "Contacto no encontrado.");

        if (!Enum.TryParse<ContactType>(command.Type, true, out var contactType))
            throw new ValidationException("OPS_INVALID_CONTACT_TYPE", "Tipo de contacto invalido.");

        contact.Update(contactType, command.Email, command.PhoneCode, command.Phone);

        contactRepository.Update(contact);
        await contactRepository.SaveChangesAsync(ct);

        return new UpdateContactResult(contact.Id, contact.Type.ToString());
    }
}
