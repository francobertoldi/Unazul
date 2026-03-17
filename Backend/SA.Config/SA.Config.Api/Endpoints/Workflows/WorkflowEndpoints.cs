using Mediator;
using SA.Config.Api.Mappers.Workflows;
using SA.Config.Api.ViewModels.Workflows;
using SA.Config.Application.Commands.Workflows;
using SA.Config.Application.Queries.Workflows;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Config.Api.Endpoints.Workflows;

public static class WorkflowEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workflows")
            .WithTags("Workflows")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-CFG-12: List workflows
        group.MapGet("/", async (
            int page,
            int page_size,
            string? search,
            string? status,
            string? sort_by,
            string? sort_dir,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new ListWorkflowsQuery(
                page,
                page_size,
                search,
                status,
                sort_by,
                sort_dir));

            return Results.Ok(WorkflowMapper.ToListResponse(
                result.Items, result.Total, result.Page, result.PageSize));
        })
        .Produces<WorkflowListResponse>(200);

        // RF-CFG-13 GET: Get workflow detail
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetWorkflowDetailQuery(id));
                return Results.Ok(WorkflowMapper.ToDetailResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "WORKFLOW_NOT_FOUND")
            {
                return Results.NotFound();
            }
        })
        .Produces<WorkflowDetailResponse>(200)
        .Produces(404);

        // RF-CFG-13 POST: Create workflow
        group.MapPost("/", async (
            CreateWorkflowRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new CreateWorkflowCommand(
                    currentUser.TenantId,
                    request.Name,
                    request.Description,
                    request.States.Select(s => new WorkflowStateInput(
                        s.Name,
                        s.Label,
                        s.Type,
                        s.PositionX,
                        s.PositionY,
                        s.Configs?.Select(c => new StateConfigInput(c.Key, c.Value)).ToArray(),
                        s.Fields?.Select(f => new StateFieldInput(f.FieldName, f.FieldType, f.IsRequired, f.SortOrder)).ToArray()
                    )).ToArray(),
                    request.Transitions.Select(t => new WorkflowTransitionInput(
                        t.FromStateIndex,
                        t.ToStateIndex,
                        t.Label,
                        t.Condition,
                        t.SlaHours
                    )).ToArray(),
                    currentUser.UserId));

                return Results.Created($"/api/v1/workflows/{result.Id}",
                    WorkflowMapper.ToActivateResponse(result));
            }
            catch (InvalidOperationException ex)
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, "WORKFLOW_CREATE_FAILED"),
                    statusCode: 422);
            }
        })
        .Produces<ActivateResponse>(201)
        .Produces<ErrorResponse>(422);

        // RF-CFG-13 PUT: Update workflow
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateWorkflowRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateWorkflowCommand(
                    id,
                    request.Name,
                    request.Description,
                    request.States.Select(s => new WorkflowStateInput(
                        s.Name,
                        s.Label,
                        s.Type,
                        s.PositionX,
                        s.PositionY,
                        s.Configs?.Select(c => new StateConfigInput(c.Key, c.Value)).ToArray(),
                        s.Fields?.Select(f => new StateFieldInput(f.FieldName, f.FieldType, f.IsRequired, f.SortOrder)).ToArray()
                    )).ToArray(),
                    request.Transitions.Select(t => new WorkflowTransitionInput(
                        t.FromStateIndex,
                        t.ToStateIndex,
                        t.Label,
                        t.Condition,
                        t.SlaHours
                    )).ToArray(),
                    currentUser.UserId));

                return Results.Ok(WorkflowMapper.ToActivateResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "WORKFLOW_NOT_FOUND")
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, "WORKFLOW_UPDATE_FAILED"),
                    statusCode: 422);
            }
        })
        .Produces<ActivateResponse>(200)
        .Produces(404)
        .Produces<ErrorResponse>(422);

        // RF-CFG-17: Activate workflow
        group.MapPut("/{id:guid}/activate", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new ActivateWorkflowCommand(id, currentUser.UserId));
                return Results.Ok(WorkflowMapper.ToActivateResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "WORKFLOW_NOT_FOUND")
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex) when (ex.Message == "WORKFLOW_ALREADY_ACTIVE")
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, "WORKFLOW_ALREADY_ACTIVE"),
                    statusCode: 409);
            }
            catch (WorkflowValidationException ex)
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, "WORKFLOW_VALIDATION_FAILED", ex.Errors),
                    statusCode: 422);
            }
        })
        .Produces<ActivateResponse>(200)
        .Produces(404)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(422);

        // RF-CFG-18: Deactivate workflow
        group.MapPut("/{id:guid}/deactivate", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new DeactivateWorkflowCommand(id, currentUser.UserId));
                return Results.Ok(WorkflowMapper.ToActivateResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "WORKFLOW_NOT_FOUND")
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex) when (ex.Message == "WORKFLOW_NOT_ACTIVE")
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, "WORKFLOW_NOT_ACTIVE"),
                    statusCode: 409);
            }
        })
        .Produces<ActivateResponse>(200)
        .Produces(404)
        .Produces<ErrorResponse>(409);
    }
}
