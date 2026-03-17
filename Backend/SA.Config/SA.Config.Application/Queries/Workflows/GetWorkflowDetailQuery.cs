using Mediator;
using SA.Config.Application.Dtos.Workflows;

namespace SA.Config.Application.Queries.Workflows;

public readonly record struct GetWorkflowDetailQuery(Guid Id) : IQuery<WorkflowDetailDto>;
