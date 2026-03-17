using Mediator;
using SA.Config.Api.Mappers.Parameters;
using SA.Config.Api.ViewModels.Parameters;
using SA.Config.Application.Commands.Parameters;
using SA.Config.Application.Queries.Parameters;
using Shared.Contract.Models;

namespace SA.Config.Api.Endpoints.Parameters;

public static class ParameterGroupEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/parameter-groups")
            .WithTags("ParameterGroups")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-CFG-01: List parameter groups by category
        group.MapGet("/", async (
            IMediator mediator) =>
        {
            var result = await mediator.Send(new ListParameterGroupsQuery());

            return Results.Ok(ParameterMapper.ToCategoryResponses(result));
        })
        .Produces<IReadOnlyList<CategoryResponse>>(200);

        // RF-CFG-06: Create parameter group
        group.MapPost("/", async (
            CreateParameterGroupRequest request,
            IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new CreateParameterGroupCommand(
                    request.Code,
                    request.Name,
                    request.Category,
                    request.Icon,
                    request.SortOrder));

                return Results.Created($"/api/v1/parameter-groups/{result.Id}",
                    ParameterMapper.ToParameterGroupResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_DUPLICATE_GROUP_CODE")
            {
                return Results.Json(
                    new ErrorResponse("El codigo de grupo ya existe.", "CFG_DUPLICATE_GROUP_CODE"),
                    statusCode: 409);
            }
        })
        .Produces<ParameterGroupResponse>(201)
        .Produces<ErrorResponse>(409);

        // RF-CFG-06: Delete parameter group
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new DeleteParameterGroupCommand(id));
                return Results.NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_GROUP_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Grupo de parametros no encontrado.", "CFG_GROUP_NOT_FOUND"),
                    statusCode: 404);
            }
            catch (InvalidOperationException ex) when (ex.Message == "CFG_GROUP_HAS_PARAMETERS")
            {
                return Results.Json(
                    new ErrorResponse("El grupo tiene parametros asociados y no puede eliminarse.", "CFG_GROUP_HAS_PARAMETERS"),
                    statusCode: 409);
            }
        })
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409);
    }
}
