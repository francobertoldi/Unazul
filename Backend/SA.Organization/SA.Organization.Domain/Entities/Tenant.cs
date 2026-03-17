using Shared.Contract.Enums;

namespace SA.Organization.Domain.Entities;

public sealed class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Identifier { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Province { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Country { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? LogoUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Tenant() { }

    public static Tenant Create(
        string name,
        string identifier,
        TenantStatus status,
        string? address,
        string? city,
        string? province,
        string? zipCode,
        string? country,
        string? phone,
        string? email,
        string? logoUrl)
    {
        return new Tenant
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Identifier = identifier,
            Status = status,
            Address = address,
            City = city,
            Province = province,
            ZipCode = zipCode,
            Country = country,
            Phone = phone,
            Email = email,
            LogoUrl = logoUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        TenantStatus status,
        string? address,
        string? city,
        string? province,
        string? zipCode,
        string? country,
        string? phone,
        string? email,
        string? logoUrl)
    {
        Name = name;
        Status = status;
        Address = address;
        City = city;
        Province = province;
        ZipCode = zipCode;
        Country = country;
        Phone = phone;
        Email = email;
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
