using Mediator;
using SA.Organization.Application.Dtos.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Tenants;

public sealed class UpdateTenantCommandHandler(
    ITenantRepository tenantRepository) : ICommandHandler<UpdateTenantCommand, TenantDto>
{
    public async ValueTask<TenantDto> Handle(UpdateTenantCommand command, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ORG_TENANT_NOT_FOUND", "Tenant no encontrado.");

        if (!Enum.TryParse<TenantStatus>(command.Status, true, out var status))
        {
            throw new ValidationException("ORG_INVALID_STATUS", "El estado es inválido.");
        }

        tenant.Update(
            command.Name,
            status,
            command.Address,
            command.City,
            command.Province,
            command.ZipCode,
            command.Country,
            command.Phone,
            command.Email,
            command.LogoUrl);

        tenantRepository.Update(tenant);
        await tenantRepository.SaveChangesAsync(ct);

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Identifier,
            tenant.Status.ToString(),
            tenant.Address,
            tenant.City,
            tenant.Province,
            tenant.Phone,
            tenant.Email,
            tenant.CreatedAt);
    }
}
