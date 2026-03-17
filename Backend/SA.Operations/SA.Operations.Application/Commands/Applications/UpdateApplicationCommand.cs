using Mediator;

namespace SA.Operations.Application.Commands.Applications;

public readonly record struct UpdateApplicationCommand(
    Guid ApplicationId,
    Guid TenantId,
    Guid? EntityId,
    Guid? ProductId,
    Guid? PlanId,
    string? FirstName,
    string? LastName,
    DateOnly? BirthDate,
    string? Gender,
    string? Occupation,
    CreateContactInput[]? Contacts,
    CreateAddressInput[]? Addresses,
    CreateBeneficiaryInput[]? Beneficiaries,
    Guid UpdatedBy) : ICommand<UpdateApplicationResult>;

public sealed record UpdateApplicationResult(Guid Id, string Code, string Status);
