using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Applicants;

public sealed record ApplicantResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DocumentType DocumentType,
    string DocumentNumber,
    DateOnly? BirthDate,
    Gender? Gender,
    string? Occupation,
    IReadOnlyList<ContactResponse> Contacts,
    IReadOnlyList<AddressResponse> Addresses,
    int ApplicationCount);

public sealed record ContactResponse(
    Guid Id,
    ContactType Type,
    string? Email,
    string? PhoneCode,
    string? Phone);

public sealed record AddressResponse(
    Guid Id,
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
