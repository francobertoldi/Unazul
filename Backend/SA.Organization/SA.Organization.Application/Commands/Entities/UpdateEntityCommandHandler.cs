using Mediator;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Entities;

public sealed class UpdateEntityCommandHandler(
    IEntityRepository entityRepository,
    IEntityChannelRepository channelRepository) : ICommandHandler<UpdateEntityCommand, EntityDetailDto>
{
    public async ValueTask<EntityDetailDto> Handle(UpdateEntityCommand command, CancellationToken ct)
    {
        var entity = await entityRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ORG_ENTITY_NOT_FOUND", "Entidad no encontrada.");

        if (!Enum.TryParse<EntityType>(command.Type, true, out var type))
        {
            throw new ValidationException("ORG_INVALID_ENTITY_TYPE", "El tipo de entidad es inválido.");
        }

        if (!Enum.TryParse<EntityStatus>(command.Status, true, out var status))
        {
            throw new ValidationException("ORG_INVALID_STATUS", "El estado es inválido.");
        }

        entity.Update(
            command.Name,
            type,
            status,
            command.Address,
            command.City,
            command.Province,
            command.ZipCode,
            command.Country,
            command.Phone,
            command.Email);

        entityRepository.Update(entity);

        // Channel diff sync
        if (command.Channels is not null)
        {
            var existingChannels = await channelRepository.GetByEntityIdAsync(entity.Id, ct);

            var newChannelTypes = command.Channels
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(ch =>
                {
                    if (!Enum.TryParse<ChannelType>(ch, true, out var channelType))
                    {
                        throw new ValidationException("ORG_INVALID_CHANNEL_TYPE", "El tipo de canal es inválido.");
                    }
                    return channelType;
                })
                .ToHashSet();

            var toRemove = existingChannels
                .Where(ec => !newChannelTypes.Contains(ec.ChannelType))
                .ToList();

            var existingTypes = existingChannels
                .Select(ec => ec.ChannelType)
                .ToHashSet();

            var toAdd = newChannelTypes
                .Where(ct2 => !existingTypes.Contains(ct2))
                .Select(ct2 => EntityChannel.Create(entity.Id, entity.TenantId, ct2))
                .ToList();

            if (toRemove.Count > 0)
            {
                channelRepository.RemoveRange(toRemove);
            }

            if (toAdd.Count > 0)
            {
                await channelRepository.AddRangeAsync(toAdd, ct);
            }

            await channelRepository.SaveChangesAsync(ct);
        }

        var full = await entityRepository.GetByIdWithDetailsAsync(entity.Id, ct);

        return EntityMapper.ToDetailDto(full!);
    }
}
