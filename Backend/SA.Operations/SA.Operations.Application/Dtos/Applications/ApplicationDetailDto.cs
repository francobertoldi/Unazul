namespace SA.Operations.Application.Dtos.Applications;

public sealed record ApplicationDetailDto(
    Guid Id,
    string Code,
    string Status,
    string? WorkflowStage,
    Guid TenantId,
    Guid EntityId,
    Guid ProductId,
    string ProductName,
    Guid PlanId,
    string PlanName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ApplicantDto Applicant,
    ContactDto[] Contacts,
    AddressDto[] Addresses,
    BeneficiaryDto[] Beneficiaries,
    DocumentDto[] Documents,
    ObservationDto[] Observations,
    TraceEventDto[] TraceEvents);
