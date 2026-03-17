using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Applicants;

public sealed record CreateContactRequest(
    ContactType Type,
    string? Email,
    string? PhoneCode,
    string? Phone);

public sealed record UpdateContactRequest(
    ContactType Type,
    string? Email,
    string? PhoneCode,
    string? Phone);

public sealed record CreateAddressRequest(
    AddressType Type,
    string Street,
    string Number,
    string? Floor,
    string? Apartment,
    string City,
    string Province,
    string PostalCode,
    decimal? Latitude,
    decimal? Longitude);

public sealed record UpdateAddressRequest(
    AddressType Type,
    string Street,
    string Number,
    string? Floor,
    string? Apartment,
    string City,
    string Province,
    string PostalCode,
    decimal? Latitude,
    decimal? Longitude);
