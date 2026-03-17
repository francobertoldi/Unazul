using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.NotificationTemplates;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.NotificationTemplates;

public sealed class UpdateNotificationTemplateCommandHandlerTests
{
    private readonly INotificationTemplateRepository _repo = Substitute.For<INotificationTemplateRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly UpdateNotificationTemplateCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public UpdateNotificationTemplateCommandHandlerTests()
    {
        _sut = new UpdateNotificationTemplateCommandHandler(_repo, _eventPublisher);
    }

    private static NotificationTemplate CreateTemplate(string channel = "email", string? subject = "Subject")
    {
        return NotificationTemplate.Create(
            TenantId,
            "test_code",
            "Original Name",
            channel,
            subject,
            "Original Body",
            "active",
            UserId);
    }

    [Fact]
    public async Task TP_CFG_19_05_Updates_Body_And_Name()
    {
        // Arrange
        var template = CreateTemplate();
        _repo.GetByIdAsync(template.Id, Arg.Any<CancellationToken>())
            .Returns(template);

        var command = new UpdateNotificationTemplateCommand(
            template.Id,
            "Updated Name",
            "Updated Subject",
            "<p>Updated Body</p>",
            "active",
            UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Subject.Should().Be("Updated Subject");
        result.Body.Should().Be("<p>Updated Body</p>");
        result.UpdatedBy.Should().Be(UserId);

        _repo.Received(1).Update(template);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_404_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repo.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new UpdateNotificationTemplateCommand(
            nonExistentId,
            "Name",
            "Subject",
            "Body",
            null,
            UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("NTPL_NOT_FOUND");
    }
}
