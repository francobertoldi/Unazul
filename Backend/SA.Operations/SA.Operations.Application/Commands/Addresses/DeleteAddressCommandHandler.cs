using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Addresses;

public sealed class DeleteAddressCommandHandler(
    IAddressRepository addressRepository) : ICommandHandler<DeleteAddressCommand, DeleteAddressResult>
{
    public async ValueTask<DeleteAddressResult> Handle(DeleteAddressCommand command, CancellationToken ct)
    {
        var address = await addressRepository.GetByIdAsync(command.AddressId, ct);
        if (address is null || address.TenantId != command.TenantId)
            throw new NotFoundException("OPS_ADDRESS_NOT_FOUND", "Direccion no encontrada.");

        addressRepository.Delete(address);
        await addressRepository.SaveChangesAsync(ct);

        return new DeleteAddressResult(address.Id);
    }
}
