using SA.Operations.Api.Mappers.Applicants;
using SA.Operations.Api.ViewModels.Applicants;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Operations.Api.Endpoints.Applicants;

public static class ApplicantEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/applicants")
            .WithTags("Applicants")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-OPS-02: Search applicant by document
        group.MapGet("/", async (
            DocumentType doc_type,
            string doc_number,
            IApplicantRepository repository,
            ICurrentUser currentUser) =>
        {
            var applicant = await repository.GetByDocumentAsync(
                currentUser.TenantId, doc_type, doc_number);

            if (applicant is null)
                return Results.Json(
                    new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                    statusCode: 404);

            var details = await repository.GetByIdWithDetailsAsync(applicant.Id);
            var appCount = await repository.CountApplicationsAsync(applicant.Id);

            // Details includes contacts and addresses via navigation
            return Results.Ok(ApplicantMapper.ToResponse(
                details ?? applicant,
                [],
                [],
                appCount));
        })
        .Produces<ApplicantResponse>(200)
        .Produces<ErrorResponse>(404);

        // RF-OPS-08: Create contact for applicant
        group.MapPost("/{applicantId:guid}/contacts", async (
            Guid applicantId,
            CreateContactRequest request,
            IApplicantRepository applicantRepository,
            ICurrentUser currentUser) =>
        {
            var applicant = await applicantRepository.GetByIdAsync(applicantId);
            if (applicant is null)
                return Results.Json(
                    new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                    statusCode: 404);

            var contact = ApplicantContact.Create(
                applicantId,
                currentUser.TenantId,
                request.Type,
                request.Email,
                request.PhoneCode,
                request.Phone);

            // Contacts are saved through the repository's unit of work
            await applicantRepository.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/applicants/{applicantId}/contacts/{contact.Id}",
                ApplicantMapper.ToContactResponse(contact));
        })
        .Produces<ContactResponse>(201)
        .Produces<ErrorResponse>(404);

        // Update contact
        group.MapPut("/{applicantId:guid}/contacts/{id:guid}", async (
            Guid applicantId,
            Guid id,
            UpdateContactRequest request,
            IApplicantRepository applicantRepository,
            ICurrentUser currentUser) =>
        {
            var applicant = await applicantRepository.GetByIdAsync(applicantId);
            if (applicant is null)
                return Results.Json(
                    new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                    statusCode: 404);

            // Contact update is handled through the applicant aggregate
            await applicantRepository.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);

        // Delete contact
        group.MapDelete("/{applicantId:guid}/contacts/{id:guid}", async (
            Guid applicantId,
            Guid id,
            IApplicantRepository applicantRepository) =>
        {
            var applicant = await applicantRepository.GetByIdAsync(applicantId);
            if (applicant is null)
                return Results.Json(
                    new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                    statusCode: 404);

            await applicantRepository.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);

        // Create address
        group.MapPost("/{applicantId:guid}/addresses", async (
            Guid applicantId,
            CreateAddressRequest request,
            IApplicantRepository applicantRepository,
            ICurrentUser currentUser) =>
        {
            var applicant = await applicantRepository.GetByIdAsync(applicantId);
            if (applicant is null)
                return Results.Json(
                    new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                    statusCode: 404);

            var address = ApplicantAddress.Create(
                applicantId,
                currentUser.TenantId,
                request.Type,
                request.Street,
                request.Number,
                request.Floor,
                request.Apartment,
                request.City,
                request.Province,
                request.PostalCode,
                request.Latitude,
                request.Longitude);

            await applicantRepository.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/applicants/{applicantId}/addresses/{address.Id}",
                ApplicantMapper.ToAddressResponse(address));
        })
        .Produces<AddressResponse>(201)
        .Produces<ErrorResponse>(404);

        // Update address
        group.MapPut("/{applicantId:guid}/addresses/{id:guid}", async (
            Guid applicantId,
            Guid id,
            UpdateAddressRequest request,
            IApplicantRepository applicantRepository,
            ICurrentUser currentUser) =>
        {
            var applicant = await applicantRepository.GetByIdAsync(applicantId);
            if (applicant is null)
                return Results.Json(
                    new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                    statusCode: 404);

            await applicantRepository.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);

        // Delete address
        group.MapDelete("/{applicantId:guid}/addresses/{id:guid}", async (
            Guid applicantId,
            Guid id,
            IApplicantRepository applicantRepository) =>
        {
            var applicant = await applicantRepository.GetByIdAsync(applicantId);
            if (applicant is null)
                return Results.Json(
                    new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                    statusCode: 404);

            await applicantRepository.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);
    }
}
