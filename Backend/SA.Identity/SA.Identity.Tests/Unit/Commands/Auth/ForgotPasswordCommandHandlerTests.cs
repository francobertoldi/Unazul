using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Identity.Application.Commands.Auth;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Events;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Auth;

public sealed class ForgotPasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository = Substitute.For<IPasswordResetTokenRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly ForgotPasswordCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ForgotPasswordCommandHandlerTests()
    {
        _sut = new ForgotPasswordCommandHandler(
            _userRepository,
            _passwordResetTokenRepository,
            _eventPublisher,
            NullLogger<ForgotPasswordCommandHandler>.Instance);
    }

    private static User CreateActiveUser(string email = "test@example.com")
    {
        return User.Create(TenantId, "testuser", "hash", email, "Test", "User", null, null, Guid.NewGuid());
    }

    [Fact]
    public async Task RF_SEC_04_01_ExistingEmail_SendsRecoveryLink()
    {
        // Arrange
        var user = CreateActiveUser("valid@example.com");
        _userRepository.GetByEmailAsync(TenantId, "valid@example.com", Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _sut.Handle(new ForgotPasswordCommand(TenantId, "valid@example.com"), CancellationToken.None);

        // Assert
        await _passwordResetTokenRepository.Received(1)
            .AddAsync(Arg.Any<PasswordResetToken>(), Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1)
            .PublishAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_04_03_NonExistentEmail_ReturnsSuccessWithoutSendingEmail()
    {
        // Arrange
        _userRepository.GetByEmailAsync(TenantId, "unknown@example.com", Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act - should NOT throw (opacity: always return success)
        Func<Task> act = async () => await _sut.Handle(new ForgotPasswordCommand(TenantId, "unknown@example.com"), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await _passwordResetTokenRepository.DidNotReceive()
            .AddAsync(Arg.Any<PasswordResetToken>(), Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive()
            .PublishAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
    }
}
