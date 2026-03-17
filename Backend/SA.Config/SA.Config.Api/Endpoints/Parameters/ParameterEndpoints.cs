using Mediator;
using SA.Config.Api.Mappers.Parameters;
using SA.Config.Api.ViewModels.Parameters;
using SA.Config.Application.Commands.Parameters;
using SA.Config.Application.Queries.Parameters;
using Shared.Auth;

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
            var result = await mediator.Send(new ListParametersQuery(group_id, parent_key));
            return Results.Ok(ParameterMapper.ToParameterResponses(result));
        })
        .Produces<IReadOnlyList<ParameterResponse>>(200);

        // RF-CFG-03: Create parameter
        group.MapPost("/", async (
            CreateParameterRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
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
        })
        .Produces<ParameterResponse>(201);

        // RF-CFG-04: Update parameter
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateParameterRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new UpdateParameterCommand(
                id,
                request.Value,
                request.Options?.Select(o =>
                    new UpdateParameterOptionInput(o.OptionValue, o.OptionLabel)).ToArray(),
                currentUser.UserId));

            return Results.Ok(ParameterMapper.ToParameterResponse(result));
        })
        .Produces<ParameterResponse>(200);

        // RF-CFG-05: Delete parameter
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            await mediator.Send(new DeleteParameterCommand(id));
            return Results.NoContent();
        })
        .Produces(204);
    }
}
