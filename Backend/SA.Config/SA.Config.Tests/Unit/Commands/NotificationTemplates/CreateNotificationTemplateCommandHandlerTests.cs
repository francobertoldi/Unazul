using FluentAssertions;
using NSubstitute;
using SA.Config.Application.Commands.NotificationTemplates;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.NotificationTemplates;

public sealed class CreateNotificationTemplateCommandHandlerTests
{
    private readonly INotificationTemplateRepository _repo = Substitute.For<INotificationTemplateRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly CreateNotificationTemplateCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public CreateNotificationTemplateCommandHandlerTests()
    {
        _sut = new CreateNotificationTemplateCommandHandler(_repo, _eventPublisher);
    }

    [Fact]
    public async Task TP_CFG_19_03_Creates_Email_Template_Successfully()
    {
        // Arrange
        _repo.ExistsByCodeAsync(TenantId, "welcome_email", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateNotificationTemplateCommand(
            TenantId,
            "welcome_email",
            "Welcome Email",
            "email",
            "Welcome to our platform",
            "<p>Hello {{name}}</p>",
            null,
            UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Code.Should().Be("welcome_email");
        result.Name.Should().Be("Welcome Email");
        result.Channel.Should().Be("email");
        result.Subject.Should().Be("Welcome to our platform");
        result.Body.Should().Be("<p>Hello {{name}}</p>");
        result.Status.Should().Be("active");
        result.CreatedBy.Should().Be(UserId);

        await _repo.Received(1).AddAsync(Arg.Any<NotificationTemplate>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_19_04_Creates_Sms_Template()
    {
        // Arrange
        _repo.ExistsByCodeAsync(TenantId, "otp_sms", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateNotificationTemplateCommand(
            TenantId,
            "otp_sms",
            "OTP SMS",
            "sms",
            null,
            "Your code is {{code}}",
            null,
            UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Code.Should().Be("otp_sms");
        result.Channel.Should().Be("sms");
        result.Subject.Should().BeNull();
        result.Body.Should().Be("Your code is {{code}}");
    }

    [Fact]
    public async Task TP_CFG_19_07_Returns_409_Duplicate_Code()
    {
        // Arrange
        _repo.ExistsByCodeAsync(TenantId, "existing_code", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateNotificationTemplateCommand(
            TenantId,
            "existing_code",
            "Duplicate",
            "email",
            "Subject",
            "Body",
            null,
            UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("NTPL_DUPLICATE_CODE");
    }

    [Fact]
    public async Task TP_CFG_19_09_Returns_422_Invalid_Channel()
    {
        // Arrange
        var command = new CreateNotificationTemplateCommand(
            TenantId,
            "bad_channel",
            "Bad Channel",
            "fax",
            null,
            "Body",
            null,
            UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("NTPL_INVALID_CHANNEL");
    }

    [Fact]
    public async Task TP_CFG_19_10_Returns_422_Email_Without_Subject()
    {
        // Arrange
        _repo.ExistsByCodeAsync(TenantId, "no_subject", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateNotificationTemplateCommand(
            TenantId,
            "no_subject",
            "No Subject Email",
            "email",
            null,
            "Body",
            null,
            UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("NTPL_SUBJECT_REQUIRED_FOR_EMAIL");
    }

    [Fact]
    public async Task TP_CFG_19_14_Publishes_Event()
    {
        // Arrange
        _repo.ExistsByCodeAsync(TenantId, "event_test", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateNotificationTemplateCommand(
            TenantId,
            "event_test",
            "Event Test",
            "sms",
            null,
            "Body",
            null,
            UserId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<Shared.Contract.Events.NotificationTemplateCreatedEvent>(),
            Arg.Any<CancellationToken>());
    }
}
