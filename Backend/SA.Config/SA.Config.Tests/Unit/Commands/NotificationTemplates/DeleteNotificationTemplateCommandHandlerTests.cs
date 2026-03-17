using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.NotificationTemplates;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.NotificationTemplates;

public sealed class DeleteNotificationTemplateCommandHandlerTests
{
    private readonly INotificationTemplateRepository _repo = Substitute.For<INotificationTemplateRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly DeleteNotificationTemplateCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public DeleteNotificationTemplateCommandHandlerTests()
    {
        _sut = new DeleteNotificationTemplateCommandHandler(_repo, _eventPublisher);
    }

    private static NotificationTemplate CreateTemplate()
    {
        return NotificationTemplate.Create(
            TenantId,
            "delete_code",
            "Delete Template",
            "email",
            "Subject",
            "Body",
            "active",
            UserId);
    }

    [Fact]
    public async Task TP_CFG_19_06_Deletes_Successfully()
    {
        // Arrange
        var template = CreateTemplate();
        _repo.GetByIdAsync(template.Id, Arg.Any<CancellationToken>())
            .Returns(template);
        _repo.IsReferencedByActiveWorkflowAsync(template.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.Handle(new DeleteNotificationTemplateCommand(template.Id), CancellationToken.None);

        // Assert
        await _repo.Received(1).DeleteAsync(template, Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<Shared.Contract.Events.NotificationTemplateDeletedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_19_08_Returns_409_Referenced_By_Active_Workflow()
    {
        // Arrange
        var template = CreateTemplate();
        _repo.GetByIdAsync(template.Id, Arg.Any<CancellationToken>())
            .Returns(template);
        _repo.IsReferencedByActiveWorkflowAsync(template.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new DeleteNotificationTemplateCommand(template.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("NTPL_REFERENCED_BY_WORKFLOW");

        await _repo.DidNotReceive().DeleteAsync(Arg.Any<NotificationTemplate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repo.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new DeleteNotificationTemplateCommand(nonExistentId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("NTPL_NOT_FOUND");
    }
}
