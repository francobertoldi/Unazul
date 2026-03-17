using FluentAssertions;
using SA.Config.DataAccess.EntityFramework.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using SA.Config.Tests.Integration.Fixtures;
using Xunit;

namespace SA.Config.Tests.Integration.Repositories;

public sealed class NotificationTemplateRepositoryIntegrationTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public NotificationTemplateRepositoryIntegrationTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Creates_With_Code_Uniqueness()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new NotificationTemplateRepository(db);

        var template = NotificationTemplate.Create(
            TenantId, "welcome_email", "Welcome", "email", "Subject", "Body", "active", UserId);

        // Act
        await repo.AddAsync(template);
        await repo.SaveChangesAsync();

        // Assert
        var exists = await repo.ExistsByCodeAsync(TenantId, "welcome_email");
        exists.Should().BeTrue();

        var notExists = await repo.ExistsByCodeAsync(TenantId, "nonexistent_code");
        notExists.Should().BeFalse();
    }

    [Fact]
    public async Task IsReferencedByActiveWorkflow_Returns_True()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new NotificationTemplateRepository(db);

        var template = NotificationTemplate.Create(
            TenantId, "ref_template", "Referenced", "email", "Subject", "Body", "active", UserId);
        await repo.AddAsync(template);
        await repo.SaveChangesAsync();

        // Create an active workflow with a state that references the template
        var workflow = WorkflowDefinition.Create(TenantId, "Active Flow", null, UserId);
        workflow.Activate(UserId);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var state = WorkflowState.Create(workflow.Id, TenantId, "send_email", "Send Email", FlowNodeType.SendMessage, 50, 50);
        db.WorkflowStates.Add(state);
        await db.SaveChangesAsync();

        var config = WorkflowStateConfig.Create(state.Id, TenantId, "template_id", template.Id.ToString());
        db.WorkflowStateConfigs.Add(config);
        await db.SaveChangesAsync();

        // Act
        var isReferenced = await repo.IsReferencedByActiveWorkflowAsync(template.Id);

        // Assert
        isReferenced.Should().BeTrue();

        // Also verify that a non-referenced template returns false
        var otherTemplate = NotificationTemplate.Create(
            TenantId, "unreferenced", "Not Referenced", "sms", null, "Body", "active", UserId);
        await repo.AddAsync(otherTemplate);
        await repo.SaveChangesAsync();

        var notReferenced = await repo.IsReferencedByActiveWorkflowAsync(otherTemplate.Id);
        notReferenced.Should().BeFalse();
    }
}
