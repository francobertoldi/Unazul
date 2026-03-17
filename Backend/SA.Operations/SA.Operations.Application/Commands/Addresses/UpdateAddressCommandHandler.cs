using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Addresses;

public sealed class UpdateAddressCommandHandler(
    IAddressRepository addressRepository) : ICommandHandler<UpdateAddressCommand, UpdateAddressResult>
{
    public async ValueTask<UpdateAddressResult> Handle(UpdateAddressCommand command, CancellationToken ct)
    {
        var address = await addressRepository.GetByIdAsync(command.AddressId, ct);
        if (address is null || address.TenantId != command.TenantId)
            throw new NotFoundException("OPS_ADDRESS_NOT_FOUND", "Direccion no encontrada.");

        if (!Enum.TryParse<AddressType>(command.Type, true, out var addrType))
            throw new ValidationException("OPS_INVALID_ADDRESS_TYPE", "Tipo de direccion invalido.");

        address.Update(
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

        addressRepository.Update(address);
        await addressRepository.SaveChangesAsync(ct);

        return new UpdateAddressResult(address.Id, address.Type.ToString());
    }
}
