using Mediator;
using SA.Config.Api.Mappers.Parameters;
using SA.Config.Api.ViewModels.Parameters;
using SA.Config.Application.Commands.Parameters;
using SA.Config.Application.Queries.Parameters;

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
            var result = await mediator.Send(new CreateParameterGroupCommand(
                request.Code,
                request.Name,
                request.Category,
                request.Icon,
                request.SortOrder));

            return Results.Created($"/api/v1/parameter-groups/{result.Id}",
                ParameterMapper.ToParameterGroupResponse(result));
        })
        .Produces<ParameterGroupResponse>(201);

        // RF-CFG-06: Delete parameter group
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            await mediator.Send(new DeleteParameterGroupCommand(id));
            return Results.NoContent();
        })
        .Produces(204);
    }
}
