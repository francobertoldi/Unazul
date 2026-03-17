using Mediator;
using SA.Organization.Application.Dtos.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Tenants;

public sealed class CreateTenantCommandHandler(
    ITenantRepository tenantRepository) : ICommandHandler<CreateTenantCommand, TenantDto>
{
    public async ValueTask<TenantDto> Handle(CreateTenantCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<TenantStatus>(command.Status, true, out var status))
        {
            throw new ValidationException("ORG_INVALID_STATUS", "El estado es inválido.");
        }

        var exists = await tenantRepository.ExistsByIdentifierAsync(command.Identifier, ct);
        if (exists)
        {
            throw new ConflictException("ORG_DUPLICATE_IDENTIFIER", "El identificador del tenant ya existe.");
        }

        var tenant = Tenant.Create(
            command.Name,
            command.Identifier,
            status,
            command.Address,
            command.City,
            command.Province,
            command.ZipCode,
            command.Country,
            command.Phone,
            command.Email,
            command.LogoUrl);

        await tenantRepository.AddAsync(tenant, ct);
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
