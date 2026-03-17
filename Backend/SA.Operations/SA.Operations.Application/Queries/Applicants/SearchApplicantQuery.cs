using Mediator;
using SA.Operations.Application.Dtos.Applications;

namespace SA.Operations.Application.Queries.Applicants;

public readonly record struct SearchApplicantQuery(
    Guid TenantId,
    string DocumentType,
    string DocumentNumber) : IQuery<ApplicantDto?>;
