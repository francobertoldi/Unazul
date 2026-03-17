using Mediator;
using SA.Config.Api.Mappers.ExternalServices;
using SA.Config.Api.ViewModels.ExternalServices;
using SA.Config.Application.Commands.ExternalServices;
using SA.Config.Application.Dtos.ExternalServices;
using SA.Config.Application.Queries.ExternalServices;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Config.Api.Endpoints.ExternalServices;

public static class ExternalServiceEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/external-services")
            .WithTags("ExternalServices")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-CFG-08: List external services
        group.MapGet("/", async (
            IMediator mediator) =>
        {
            var result = await mediator.Send(new ListExternalServicesQuery());
            return Results.Ok(ExternalServiceMapper.ToResponseList(result));
        })
        .Produces<List<ExternalServiceResponse>>(200);

        // RF-CFG-09: Create external service
        group.MapPost("/", async (
            CreateExternalServiceRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new CreateExternalServiceCommand(
                    currentUser.TenantId,
                    request.Name,
                    request.Description,
                    request.Type,
                    request.BaseUrl,
                    request.Status,
                    request.TimeoutMs,
                    request.MaxRetries,
                    request.AuthType,
                    request.AuthConfigs?.Select(c =>
                        new AuthConfigInput(c.Key, c.Value)).ToArray(),
                    currentUser.UserId));

                return Results.Created($"/api/v1/external-services/{result.Id}",
                    ExternalServiceMapper.ToResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_DUPLICATE_SERVICE_NAME")
            {
                return Results.Json(
                    new ErrorResponse("El nombre del servicio ya existe.", "CFG_DUPLICATE_SERVICE_NAME"),
                    statusCode: 409);
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_INVALID_AUTH_CONFIG")
            {
                return Results.Json(
                    new ErrorResponse("Configuracion de autenticacion invalida para el tipo especificado.", "CFG_INVALID_AUTH_CONFIG"),
                    statusCode: 422);
            }
        })
        .Produces<ExternalServiceResponse>(201)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(422);

        // RF-CFG-10: Update external service
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateExternalServiceRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateExternalServiceCommand(
                    id,
                    request.Name,
                    request.Description,
                    request.Type,
                    request.BaseUrl,
                    request.Status,
                    request.TimeoutMs,
                    request.MaxRetries,
                    request.AuthType,
                    request.AuthConfigs?.Select(c =>
                        new AuthConfigInput(c.Key, c.Value)).ToArray(),
                    currentUser.UserId));

                return Results.Ok(ExternalServiceMapper.ToResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_SERVICE_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Servicio externo no encontrado.", "CFG_SERVICE_NOT_FOUND"),
                    statusCode: 404);
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_DUPLICATE_SERVICE_NAME")
            {
                return Results.Json(
                    new ErrorResponse("El nombre del servicio ya existe.", "CFG_DUPLICATE_SERVICE_NAME"),
                    statusCode: 409);
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_INVALID_AUTH_CONFIG")
            {
                return Results.Json(
                    new ErrorResponse("Configuracion de autenticacion invalida para el tipo especificado.", "CFG_INVALID_AUTH_CONFIG"),
                    statusCode: 422);
            }
        })
        .Produces<ExternalServiceResponse>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(422);

        // RF-CFG-11: Test external service connection
        group.MapPost("/{id:guid}/test", async (
            Guid id,
            IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new TestExternalServiceCommand(id));
                return Results.Ok(ExternalServiceMapper.ToTestConnectionResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_SERVICE_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Servicio externo no encontrado.", "CFG_SERVICE_NOT_FOUND"),
                    statusCode: 404);
            }
        })
        .Produces<TestConnectionResponse>(200)
        .Produces<ErrorResponse>(404);
    }
}
