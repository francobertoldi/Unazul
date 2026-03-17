using SA.Operations.Domain.Enums;

namespace SA.Operations.Domain.Entities;

public sealed class ApplicantAddress
{
    public Guid Id { get; private set; }
    public Guid ApplicantId { get; private set; }
    public Guid TenantId { get; private set; }
    public AddressType Type { get; private set; }
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string? Floor { get; private set; }
    public string? Apartment { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string Province { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    private ApplicantAddress() { }

    public static ApplicantAddress Create(
        Guid applicantId,
        Guid tenantId,
        AddressType type,
        string street,
        string number,
        string? floor,
        string? apartment,
        string city,
        string province,
        string postalCode,
        decimal? latitude,
        decimal? longitude)
    {
        return new ApplicantAddress
        {
            Id = Guid.CreateVersion7(),
            ApplicantId = applicantId,
            TenantId = tenantId,
            Type = type,
            Street = street,
            Number = number,
            Floor = floor,
            Apartment = apartment,
            City = city,
            Province = province,
            PostalCode = postalCode,
            Latitude = latitude,
            Longitude = longitude
        };
    }

    public void Update(
        AddressType type,
        string street,
        string number,
        string? floor,
        string? apartment,
        string city,
        string province,
        string postalCode,
        decimal? latitude,
        decimal? longitude)
    {
        Type = type;
        Street = street;
        Number = number;
        Floor = floor;
        Apartment = apartment;
        City = city;
        Province = province;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
    }
}
