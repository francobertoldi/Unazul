using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Operations.Application.Commands.Messages;
using SA.Operations.Application.Dtos.Messages;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Contract.Events;
using Xunit;
using AppEntity = SA.Operations.Domain.Entities.Application;

namespace SA.Operations.Tests.Unit.Commands.Messages;

public sealed class SendMessageCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IObservationRepository _observationRepository = Substitute.For<IObservationRepository>();
    private readonly IConfigServiceClient _configClient = Substitute.For<IConfigServiceClient>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly SendMessageCommandHandler _sut;

    public SendMessageCommandHandlerTests()
    {
        _sut = new SendMessageCommandHandler(_applicationRepository, _observationRepository, _configClient, _eventPublisher);
    }

    private static AppEntity CreateApplication(Guid tenantId)
    {
        return AppEntity.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), "OPS-001", Guid.NewGuid(), Guid.NewGuid(), "Product", "Plan", Guid.NewGuid());
    }

    private static NotificationTemplateResult CreateTemplate(string content = "Hello {{name}}", string? subject = "Welcome {{name}}")
    {
        return new NotificationTemplateResult(Guid.NewGuid(), "Welcome Template", subject, content, "Email");
    }

    [Fact]
    public async Task TP_OPS_15_01_SendMessage_WithSentStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var templateId = Guid.NewGuid();
        var template = CreateTemplate();
        var variables = new Dictionary<string, string> { { "name", "John" } };
        var command = new SendMessageCommand(app.Id, tenantId, templateId, "Email", "john@test.com", variables, Guid.NewGuid(), "Admin");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _configClient.GetNotificationTemplateAsync(templateId, Arg.Any<CancellationToken>()).Returns(template);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Sent");
        result.Channel.Should().Be("Email");
        result.Recipient.Should().Be("john@test.com");
        result.MessageId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_OPS_15_02_ThrowsOpsApplicationNotFound_WhenMissing()
    {
        // Arrange
        var command = new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Email", "john@test.com", null, Guid.NewGuid(), "Admin");

        _applicationRepository.GetByIdAsync(command.ApplicationId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICATION_NOT_FOUND");
    }

    [Fact]
    public async Task TP_OPS_15_03_ThrowsOpsTemplateNotFound_WhenMissing()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var command = new SendMessageCommand(app.Id, tenantId, Guid.NewGuid(), "Email", "john@test.com", null, Guid.NewGuid(), "Admin");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _configClient.GetNotificationTemplateAsync(command.TemplateId, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_TEMPLATE_NOT_FOUND");
    }

    [Fact]
    public async Task TP_OPS_15_04_ResolvesTemplateVariables()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var templateId = Guid.NewGuid();
        var template = new NotificationTemplateResult(templateId, "Welcome", "Subject for {{name}}", "Hello {{name}}, your code is {{code}}", "Email");
        var variables = new Dictionary<string, string> { { "name", "John" }, { "code", "OPS-001" } };
        var command = new SendMessageCommand(app.Id, tenantId, templateId, "Email", "john@test.com", variables, Guid.NewGuid(), "Admin");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _configClient.GetNotificationTemplateAsync(templateId, Arg.Any<CancellationToken>()).Returns(template);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert - verify the observation was created with resolved content
        await _observationRepository.Received(1).AddAsync(
            Arg.Is<ApplicationObservation>(o => o.Content == "Hello John, your code is OPS-001"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_OPS_15_05_PublishesMessageSentEvent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var templateId = Guid.NewGuid();
        var template = CreateTemplate("Hello World", "Subject");
        var command = new SendMessageCommand(app.Id, tenantId, templateId, "Email", "john@test.com", null, Guid.NewGuid(), "Admin");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _configClient.GetNotificationTemplateAsync(templateId, Arg.Any<CancellationToken>()).Returns(template);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<MessageSentEvent>(e =>
                e.TenantId == tenantId &&
                e.ApplicationId == app.Id &&
                e.Channel == "Email" &&
                e.Recipient == "john@test.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_OPS_15_06_CreatesObservationWithMessageType()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var templateId = Guid.NewGuid();
        var template = CreateTemplate("Hello World", null);
        var command = new SendMessageCommand(app.Id, tenantId, templateId, "Email", "john@test.com", null, Guid.NewGuid(), "Admin");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _configClient.GetNotificationTemplateAsync(templateId, Arg.Any<CancellationToken>()).Returns(template);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _observationRepository.Received(1).AddAsync(
            Arg.Is<ApplicationObservation>(o => o.ObservationType == ObservationType.Message),
            Arg.Any<CancellationToken>());
        await _observationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
