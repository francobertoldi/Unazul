using Mediator;
using SA.Operations.Application.Dtos.Applications;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Queries.Applicants;

public sealed class SearchApplicantQueryHandler(
    IApplicantRepository applicantRepository) : IQueryHandler<SearchApplicantQuery, ApplicantDto?>
{
    public async ValueTask<ApplicantDto?> Handle(SearchApplicantQuery query, CancellationToken ct)
    {
        if (!Enum.TryParse<DocumentType>(query.DocumentType, true, out var docType))
            throw new ValidationException("OPS_INVALID_DOCUMENT_TYPE", "Tipo de documento invalido.");

        var applicant = await applicantRepository.GetByDocumentAsync(query.TenantId, docType, query.DocumentNumber, ct);
        if (applicant is null)
            return null;

        var applicationCount = await applicantRepository.CountApplicationsAsync(applicant.Id, ct);

        return new ApplicantDto(
            applicant.Id,
            applicant.FirstName,
            applicant.LastName,
            applicant.DocumentType.ToString(),
            applicant.DocumentNumber,
            applicant.BirthDate,
            applicant.Gender?.ToString(),
            applicant.Occupation,
            applicationCount);
    }
}
