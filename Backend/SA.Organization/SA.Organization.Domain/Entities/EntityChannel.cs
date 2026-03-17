using Shared.Contract.Enums;

namespace SA.Organization.Domain.Entities;

public sealed class EntityChannel
{
    public Guid Id { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid TenantId { get; private set; }
    public ChannelType ChannelType { get; private set; }
    public bool IsActive { get; private set; }

    private EntityChannel() { }

    public static EntityChannel Create(
        Guid entityId,
        Guid tenantId,
        ChannelType channelType,
        bool isActive = true)
    {
        return new EntityChannel
        {
            Id = Guid.CreateVersion7(),
            EntityId = entityId,
            TenantId = tenantId,
            ChannelType = channelType,
            IsActive = isActive
        };
    }
}
