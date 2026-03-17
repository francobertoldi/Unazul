using Mediator;

namespace SA.Operations.Application.Commands.Applications;

public readonly record struct CreateApplicationCommand(
    Guid TenantId,
    Guid EntityId,
    Guid ProductId,
    Guid PlanId,
    string FirstName,
    string LastName,
    string DocumentType,
    string DocumentNumber,
    DateOnly? BirthDate,
    string? Gender,
    string? Occupation,
    CreateContactInput[]? Contacts,
    CreateAddressInput[]? Addresses,
    CreateBeneficiaryInput[]? Beneficiaries,
    Guid CreatedBy,
    string CreatedByName) : ICommand<CreateApplicationResult>;

public sealed record CreateApplicationResult(Guid Id, string Code, string Status);

public sealed record CreateContactInput(string Type, string? Email, string? PhoneCode, string? Phone);
public sealed record CreateAddressInput(string Type, string Street, string Number, string? Floor, string? Apartment, string City, string Province, string PostalCode, decimal? Latitude, decimal? Longitude);
public sealed record CreateBeneficiaryInput(string FirstName, string LastName, string Relationship, decimal Percentage);
