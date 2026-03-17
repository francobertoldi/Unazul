using SA.Organization.Domain.Entities;

namespace SA.Organization.Application.Dtos.Entities;

internal static class EntityMapper
{
    internal static EntityDetailDto ToDetailDto(Entity entity)
    {
        var channels = entity.Channels
            .Select(c => new EntityChannelDto(c.Id, c.ChannelType.ToString(), c.IsActive))
            .ToList();

        var branches = entity.Branches
            .Select(b => new BranchDto(
                b.Id, b.EntityId, b.Name, b.Code,
                b.Address, b.City, b.Province, b.ZipCode, b.Country,
                b.Phone, b.Email, b.IsActive, b.CreatedAt, b.UpdatedAt))
            .ToList();

        return new EntityDetailDto(
            entity.Id,
            entity.TenantId,
            entity.Name,
            entity.Cuit,
            entity.Type.ToString(),
            entity.Status.ToString(),
            entity.Address,
            entity.City,
            entity.Province,
            entity.ZipCode,
            entity.Country,
            entity.Phone,
            entity.Email,
            channels,
            branches,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
