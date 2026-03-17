using Mediator;
using SA.Operations.Application.Dtos.Applications;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Queries.Applications;

public sealed class GetApplicationDetailQueryHandler(
    IApplicationRepository applicationRepository,
    IApplicantRepository applicantRepository,
    IContactRepository contactRepository,
    IAddressRepository addressRepository,
    IBeneficiaryRepository beneficiaryRepository,
    IDocumentRepository documentRepository,
    IObservationRepository observationRepository,
    ITraceEventRepository traceEventRepository) : IQueryHandler<GetApplicationDetailQuery, ApplicationDetailDto>
{
    public async ValueTask<ApplicationDetailDto> Handle(GetApplicationDetailQuery query, CancellationToken ct)
    {
        var app = await applicationRepository.GetByIdAsync(query.ApplicationId, ct);
        if (app is null || app.TenantId != query.TenantId)
            throw new NotFoundException("OPS_APPLICATION_NOT_FOUND", "Solicitud no encontrada.");

        var applicant = await applicantRepository.GetByIdAsync(app.ApplicantId, ct)
            ?? throw new NotFoundException("OPS_APPLICANT_NOT_FOUND", "Solicitante no encontrado.");

        var applicationCount = await applicantRepository.CountApplicationsAsync(applicant.Id, ct);

        var contacts = await contactRepository.GetByApplicantIdAsync(applicant.Id, ct);
        var addresses = await addressRepository.GetByApplicantIdAsync(applicant.Id, ct);
        var beneficiaries = await beneficiaryRepository.GetByApplicationIdAsync(app.Id, ct);
        var documents = await documentRepository.GetByApplicationIdAsync(app.Id, ct);
        var observations = await observationRepository.GetByApplicationIdAsync(app.Id, ct);
        var traceEvents = await traceEventRepository.GetByApplicationIdAsync(app.Id, ct);

        return new ApplicationDetailDto(
            app.Id,
            app.Code,
            app.Status.ToString(),
            app.WorkflowStage,
            app.TenantId,
            app.EntityId,
            app.ProductId,
            app.ProductName,
            app.PlanId,
            app.PlanName,
            app.CreatedAt,
            app.UpdatedAt,
            new ApplicantDto(
                applicant.Id,
                applicant.FirstName,
                applicant.LastName,
                applicant.DocumentType.ToString(),
                applicant.DocumentNumber,
                applicant.BirthDate,
                applicant.Gender?.ToString(),
                applicant.Occupation,
                applicationCount),
            contacts.Select(c => new ContactDto(c.Id, c.Type.ToString(), c.Email, c.PhoneCode, c.Phone)).ToArray(),
            addresses.Select(a => new AddressDto(a.Id, a.Type.ToString(), a.Street, a.Number, a.Floor, a.Apartment, a.City, a.Province, a.PostalCode, a.Latitude, a.Longitude)).ToArray(),
            beneficiaries.Select(b => new BeneficiaryDto(b.Id, b.FirstName, b.LastName, b.Relationship, b.Percentage)).ToArray(),
            documents.Select(d => new DocumentDto(d.Id, d.Name, d.DocumentType, d.FileUrl, d.Status.ToString(), d.CreatedAt)).ToArray(),
            observations.Select(o => new ObservationDto(o.Id, o.ObservationType.ToString(), o.Content, o.UserName, o.CreatedAt)).ToArray(),
            traceEvents.Select(t => new TraceEventDto(
                t.Id,
                t.State,
                t.Action,
                t.UserName,
                t.OccurredAt,
                t.Details.Select(d => new TraceEventDetailDto(d.Key, d.Value)).ToArray())).ToArray());
    }
}
