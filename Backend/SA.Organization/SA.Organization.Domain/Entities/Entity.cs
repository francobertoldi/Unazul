using Shared.Contract.Enums;

namespace SA.Organization.Domain.Entities;

public sealed class Entity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Cuit { get; private set; } = string.Empty;
    public EntityType Type { get; private set; }
    public EntityStatus Status { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Province { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Country { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<EntityChannel> Channels { get; private set; } = [];
    public ICollection<Branch> Branches { get; private set; } = [];

    private Entity() { }

    public static Entity Create(
        Guid tenantId,
        string name,
        string cuit,
        EntityType type,
        EntityStatus status,
        string? address,
        string? city,
        string? province,
        string? zipCode,
        string? country,
        string? phone,
        string? email)
    {
        return new Entity
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name,
            Cuit = cuit,
            Type = type,
            Status = status,
            Address = address,
            City = city,
            Province = province,
            ZipCode = zipCode,
            Country = country,
            Phone = phone,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        EntityType type,
        EntityStatus status,
        string? address,
        string? city,
        string? province,
        string? zipCode,
        string? country,
        string? phone,
        string? email)
    {
        Name = name;
        Type = type;
        Status = status;
        Address = address;
        City = city;
        Province = province;
        ZipCode = zipCode;
        Country = country;
        Phone = phone;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}
