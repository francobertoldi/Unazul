using FluentAssertions;
using NSubstitute;
using SA.Config.Application.Queries.Workflows;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Queries.Workflows;

public sealed class ListWorkflowsQueryHandlerTests
{
    private readonly IWorkflowRepository _repo = Substitute.For<IWorkflowRepository>();
    private readonly ListWorkflowsQueryHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ListWorkflowsQueryHandlerTests()
    {
        _sut = new ListWorkflowsQueryHandler(_repo);
    }

    private static WorkflowDefinition CreateWorkflow(string name = "TestWorkflow")
    {
        return WorkflowDefinition.Create(TenantId, name, "desc", Guid.NewGuid());
    }

    [Fact]
    public async Task TP_CFG_12_01_Returns_Paginated_List()
    {
        // Arrange
        var workflows = new List<WorkflowDefinition> { CreateWorkflow("WF1"), CreateWorkflow("WF2") };
        _repo.ListAsync(0, 10, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((workflows.AsReadOnly(), 2));

        var query = new ListWorkflowsQuery(1, 10, null, null, null, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task TP_CFG_12_06_Clamps_PageSize_To_100()
    {
        // Arrange
        _repo.ListAsync(0, 100, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<WorkflowDefinition>().AsReadOnly(), 0));

        var query = new ListWorkflowsQuery(1, 500, null, null, null, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(100);
        await _repo.Received(1).ListAsync(0, 100, null, null, null, null, Arg.Any<CancellationToken>());
    }
}
