using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Contacts;

public sealed class CreateContactCommandHandler(
    IApplicantRepository applicantRepository,
    IContactRepository contactRepository) : ICommandHandler<CreateContactCommand, CreateContactResult>
{
    public async ValueTask<CreateContactResult> Handle(CreateContactCommand command, CancellationToken ct)
    {
        var applicant = await applicantRepository.GetByIdAsync(command.ApplicantId, ct);
        if (applicant is null || applicant.TenantId != command.TenantId)
            throw new NotFoundException("OPS_APPLICANT_NOT_FOUND", "Solicitante no encontrado.");

        if (!Enum.TryParse<ContactType>(command.Type, true, out var contactType))
            throw new ValidationException("OPS_INVALID_CONTACT_TYPE", "Tipo de contacto invalido.");

        var contact = ApplicantContact.Create(
            command.ApplicantId,
            command.TenantId,
            contactType,
            command.Email,
            command.PhoneCode,
            command.Phone);

        await contactRepository.AddAsync(contact, ct);
        await contactRepository.SaveChangesAsync(ct);

        return new CreateContactResult(contact.Id, contact.Type.ToString());
    }
}
