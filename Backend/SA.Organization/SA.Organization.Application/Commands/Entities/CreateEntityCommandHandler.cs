using System.Text.RegularExpressions;
using Mediator;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Entities;

public sealed partial class CreateEntityCommandHandler(
    IEntityRepository entityRepository,
    IEntityChannelRepository channelRepository) : ICommandHandler<CreateEntityCommand, EntityDetailDto>
{
    [GeneratedRegex(@"^\d{2}-\d{8}-\d{1}$")]
    private static partial Regex CuitRegex();

    public async ValueTask<EntityDetailDto> Handle(CreateEntityCommand command, CancellationToken ct)
    {
        if (!CuitRegex().IsMatch(command.Cuit))
        {
            throw new ValidationException("ORG_INVALID_CUIT_FORMAT", "El formato del CUIT es inválido.");
        }

        if (!Enum.TryParse<EntityType>(command.Type, true, out var type))
        {
            throw new ValidationException("ORG_INVALID_ENTITY_TYPE", "El tipo de entidad es inválido.");
        }

        if (!Enum.TryParse<EntityStatus>(command.Status, true, out var status))
        {
            throw new ValidationException("ORG_INVALID_STATUS", "El estado es inválido.");
        }

        var exists = await entityRepository.ExistsByCuitAsync(command.TenantId, command.Cuit, ct);
        if (exists)
        {
            throw new ConflictException("ORG_DUPLICATE_CUIT", "El CUIT ya existe.");
        }

        var entity = Entity.Create(
            command.TenantId,
            command.Name,
            command.Cuit,
            type,
            status,
            command.Address,
            command.City,
            command.Province,
            command.ZipCode,
            command.Country,
            command.Phone,
            command.Email);

        await entityRepository.AddAsync(entity, ct);

        if (command.Channels is { Length: > 0 })
        {
            var uniqueChannels = command.Channels.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var channelEntities = new List<EntityChannel>();

            foreach (var ch in uniqueChannels)
            {
                if (!Enum.TryParse<ChannelType>(ch, true, out var channelType))
                {
                    throw new ValidationException("ORG_INVALID_CHANNEL_TYPE", "El tipo de canal es inválido.");
                }

                channelEntities.Add(EntityChannel.Create(entity.Id, command.TenantId, channelType));
            }

            await channelRepository.AddRangeAsync(channelEntities, ct);
            await channelRepository.SaveChangesAsync(ct);
        }

        var full = await entityRepository.GetByIdWithDetailsAsync(entity.Id, ct);

        return EntityMapper.ToDetailDto(full!);
    }
}
