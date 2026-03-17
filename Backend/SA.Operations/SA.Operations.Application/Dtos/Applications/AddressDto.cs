namespace SA.Operations.Application.Dtos.Applications;

public sealed record AddressDto(
    Guid Id,
    string Type,
    string Street,
    string Number,
    string? Floor,
    string? Apartment,
    string City,
    string Province,
    string PostalCode,
    decimal? Latitude,
    decimal? Longitude);
