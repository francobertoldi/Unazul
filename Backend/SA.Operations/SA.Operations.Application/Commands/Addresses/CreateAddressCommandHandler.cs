using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Addresses;

public sealed class CreateAddressCommandHandler(
    IApplicantRepository applicantRepository,
    IAddressRepository addressRepository) : ICommandHandler<CreateAddressCommand, CreateAddressResult>
{
    public async ValueTask<CreateAddressResult> Handle(CreateAddressCommand command, CancellationToken ct)
    {
        var applicant = await applicantRepository.GetByIdAsync(command.ApplicantId, ct);
        if (applicant is null || applicant.TenantId != command.TenantId)
            throw new NotFoundException("OPS_APPLICANT_NOT_FOUND", "Solicitante no encontrado.");

        if (!Enum.TryParse<AddressType>(command.Type, true, out var addrType))
            throw new ValidationException("OPS_INVALID_ADDRESS_TYPE", "Tipo de direccion invalido.");

        var address = ApplicantAddress.Create(
            command.ApplicantId,
            command.TenantId,
            addrType,
            command.Street,
            command.Number,
            command.Floor,
            command.Apartment,
            command.City,
            command.Province,
            command.PostalCode,
            command.Latitude,
            command.Longitude);

        await addressRepository.AddAsync(address, ct);
        await addressRepository.SaveChangesAsync(ct);

        return new CreateAddressResult(address.Id, address.Type.ToString());
    }
}
