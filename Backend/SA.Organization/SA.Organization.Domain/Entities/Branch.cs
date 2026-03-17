using Shared.Contract.Enums;

namespace SA.Organization.Domain.Entities;

public sealed class Branch
{
    public Guid Id { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Province { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Country { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Branch() { }

    public static Branch Create(
        Guid entityId,
        Guid tenantId,
        string name,
        string code,
        string? address,
        string? city,
        string? province,
        string? zipCode,
        string? country,
        string? phone,
        string? email,
        bool isActive = true)
    {
        return new Branch
        {
            Id = Guid.CreateVersion7(),
            EntityId = entityId,
            TenantId = tenantId,
            Name = name,
            Code = code,
            Address = address,
            City = city,
            Province = province,
            ZipCode = zipCode,
            Country = country,
            Phone = phone,
            Email = email,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? address,
        string? city,
        string? province,
        string? zipCode,
        string? country,
        string? phone,
        string? email,
        bool isActive)
    {
        Name = name;
        Address = address;
        City = city;
        Province = province;
        ZipCode = zipCode;
        Country = country;
        Phone = phone;
        Email = email;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
