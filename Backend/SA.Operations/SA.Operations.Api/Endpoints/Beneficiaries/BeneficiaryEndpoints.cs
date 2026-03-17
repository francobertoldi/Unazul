using SA.Operations.Api.Mappers.Beneficiaries;
using SA.Operations.Api.ViewModels.Beneficiaries;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Operations.Api.Endpoints.Beneficiaries;

public static class BeneficiaryEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/applications/{applicationId:guid}/beneficiaries")
            .WithTags("Beneficiaries")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-OPS-09: Create beneficiary
        group.MapPost("/", async (
            Guid applicationId,
            CreateBeneficiaryRequest request,
            IApplicationRepository applicationRepository,
            IBeneficiaryRepository repository,
            ICurrentUser currentUser) =>
        {
            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var currentSum = await repository.SumPercentageAsync(applicationId);
            if (currentSum + request.Percentage > 100)
                return Results.Json(
                    new ErrorResponse("La suma de porcentajes de beneficiarios excede 100%.", "OPS_BENEFICIARY_PERCENTAGE_EXCEEDED"),
                    statusCode: 422);

            var beneficiary = Beneficiary.Create(
                applicationId,
                currentUser.TenantId,
                request.FirstName,
                request.LastName,
                request.Relationship,
                request.Percentage);

            await repository.AddAsync(beneficiary);
            await repository.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/applications/{applicationId}/beneficiaries/{beneficiary.Id}",
                BeneficiaryMapper.ToResponse(beneficiary));
        })
        .Produces<BeneficiaryResponse>(201)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Update beneficiary
        group.MapPut("/{id:guid}", async (
            Guid applicationId,
            Guid id,
            UpdateBeneficiaryRequest request,
            IApplicationRepository applicationRepository,
            IBeneficiaryRepository repository) =>
        {
            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var beneficiaries = await repository.GetByApplicationIdAsync(applicationId);
            var beneficiary = beneficiaries.FirstOrDefault(b => b.Id == id);
            if (beneficiary is null)
                return Results.Json(
                    new ErrorResponse("Beneficiario no encontrado.", "OPS_BENEFICIARY_NOT_FOUND"),
                    statusCode: 404);

            var otherSum = beneficiaries.Where(b => b.Id != id).Sum(b => b.Percentage);
            if (otherSum + request.Percentage > 100)
                return Results.Json(
                    new ErrorResponse("La suma de porcentajes de beneficiarios excede 100%.", "OPS_BENEFICIARY_PERCENTAGE_EXCEEDED"),
                    statusCode: 422);

            beneficiary.Update(
                request.FirstName,
                request.LastName,
                request.Relationship,
                request.Percentage);

            repository.Update(beneficiary);
            await repository.SaveChangesAsync();

            return Results.Ok(BeneficiaryMapper.ToResponse(beneficiary));
        })
        .Produces<BeneficiaryResponse>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Delete beneficiary
        group.MapDelete("/{id:guid}", async (
            Guid applicationId,
            Guid id,
            IApplicationRepository applicationRepository,
            IBeneficiaryRepository repository) =>
        {
            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var beneficiaries = await repository.GetByApplicationIdAsync(applicationId);
            var beneficiary = beneficiaries.FirstOrDefault(b => b.Id == id);
            if (beneficiary is null)
                return Results.Json(
                    new ErrorResponse("Beneficiario no encontrado.", "OPS_BENEFICIARY_NOT_FOUND"),
                    statusCode: 404);

            repository.Delete(beneficiary);
            await repository.SaveChangesAsync();

            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);
    }
}
