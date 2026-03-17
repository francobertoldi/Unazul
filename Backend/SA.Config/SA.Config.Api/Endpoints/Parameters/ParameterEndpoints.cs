using Mediator;
using SA.Config.Api.Mappers.Parameters;
using SA.Config.Api.ViewModels.Parameters;
using SA.Config.Application.Commands.Parameters;
using SA.Config.Application.Queries.Parameters;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Config.Api.Endpoints.Parameters;

public static class ParameterEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/parameters")
            .WithTags("Parameters")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-CFG-02 / RF-CFG-07: List parameters by group
        group.MapGet("/", async (
            Guid group_id,
            string? parent_key,
            IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new ListParametersQuery(group_id, parent_key));

                return Results.Ok(ParameterMapper.ToParameterResponses(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_GROUP_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Grupo de parametros no encontrado.", "CFG_GROUP_NOT_FOUND"),
                    statusCode: 404);
            }
        })
        .Produces<IReadOnlyList<ParameterResponse>>(200)
        .Produces<ErrorResponse>(404);

        // RF-CFG-03: Create parameter
        group.MapPost("/", async (
            CreateParameterRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new CreateParameterCommand(
                    currentUser.TenantId,
                    request.GroupId,
                    request.Key,
                    request.Value,
                    request.Type,
                    request.Description,
                    request.ParentKey,
                    request.Options?.Select(o =>
                        new CreateParameterOptionInput(o.OptionValue, o.OptionLabel)).ToArray(),
                    currentUser.UserId));

                return Results.Created($"/api/v1/parameters/{result.Id}",
                    ParameterMapper.ToParameterResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_GROUP_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Grupo de parametros no encontrado.", "CFG_GROUP_NOT_FOUND"),
                    statusCode: 404);
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_DUPLICATE_KEY")
            {
                return Results.Json(
                    new ErrorResponse("La clave del parametro ya existe en este grupo.", "CFG_DUPLICATE_KEY"),
                    statusCode: 409);
            }
            catch (InvalidOperationException ex) when (ex.Message is "CFG_INVALID_NUMBER_VALUE" or "CFG_INVALID_BOOLEAN_VALUE" or "CFG_OPTIONS_REQUIRED")
            {
                return Results.Json(
                    new ErrorResponse(GetValidationMessage(ex.Message), ex.Message),
                    statusCode: 422);
            }
        })
        .Produces<ParameterResponse>(201)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(422);

        // RF-CFG-04: Update parameter
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateParameterRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateParameterCommand(
                    id,
                    request.Value,
                    request.Options?.Select(o =>
                        new UpdateParameterOptionInput(o.OptionValue, o.OptionLabel)).ToArray(),
                    currentUser.UserId));

                return Results.Ok(ParameterMapper.ToParameterResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_PARAMETER_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Parametro no encontrado.", "CFG_PARAMETER_NOT_FOUND"),
                    statusCode: 404);
            }
            catch (InvalidOperationException ex) when (ex.Message is "CFG_INVALID_NUMBER_VALUE" or "CFG_INVALID_BOOLEAN_VALUE")
            {
                return Results.Json(
                    new ErrorResponse(GetValidationMessage(ex.Message), ex.Message),
                    statusCode: 422);
            }
        })
        .Produces<ParameterResponse>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // RF-CFG-05: Delete parameter
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new DeleteParameterCommand(id));
                return Results.NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_PARAMETER_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Parametro no encontrado.", "CFG_PARAMETER_NOT_FOUND"),
                    statusCode: 404);
            }
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);
    }

    private static string GetValidationMessage(string code) => code switch
    {
        "CFG_INVALID_NUMBER_VALUE" => "El valor no es un numero valido.",
        "CFG_INVALID_BOOLEAN_VALUE" => "El valor debe ser 'true' o 'false'.",
        "CFG_OPTIONS_REQUIRED" => "Los tipos Select y List requieren opciones.",
        _ => "Error de validacion."
    };
}
