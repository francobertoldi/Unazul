using FluentAssertions;
using NSubstitute;
using SA.Config.Application.Commands.ExternalServices;
using SA.Config.Application.Dtos.ExternalServices;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.ExternalServices;

public sealed class CreateExternalServiceCommandHandlerTests
{
    private readonly IExternalServiceRepository _repo = Substitute.For<IExternalServiceRepository>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly CreateExternalServiceCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public CreateExternalServiceCommandHandlerTests()
    {
        _sut = new CreateExternalServiceCommandHandler(_repo, _encryption, _eventPublisher);
        _encryption.Encrypt(Arg.Any<string>()).Returns(x => "encrypted_" + x.Arg<string>());
        _encryption.Decrypt(Arg.Any<string>()).Returns(x => ((string)x[0]).Replace("encrypted_", ""));
    }

    [Fact]
    public async Task TP_CFG_09_01_Creates_RestApi_With_ApiKey()
    {
        // Arrange
        _repo.ExistsByNameAsync(TenantId, "MyApi", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateExternalServiceCommand(
            TenantId, "MyApi", "API description", "RestApi", "https://api.example.com",
            null, 30000, 3, "ApiKey",
            new[] { new AuthConfigInput("header_name", "X-Api-Key"), new AuthConfigInput("api_key", "secret123") },
            UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("MyApi");
        result.Type.Should().Be(ServiceType.RestApi);
        result.AuthType.Should().Be(AuthType.ApiKey);
        result.BaseUrl.Should().Be("https://api.example.com");

        await _repo.Received(1).AddAsync(Arg.Any<ExternalService>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).ReplaceAuthConfigsAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<ServiceAuthConfig>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_09_02_Creates_With_AuthType_None()
    {
        // Arrange
        _repo.ExistsByNameAsync(TenantId, "NoAuthService", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateExternalServiceCommand(
            TenantId, "NoAuthService", null, "RestApi", "https://open.api.com",
            null, null, null, "None", null, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("NoAuthService");
        result.AuthType.Should().Be(AuthType.None);

        await _repo.DidNotReceive().ReplaceAuthConfigsAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<ServiceAuthConfig>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_09_03_Creates_OAuth2_With_All_Keys()
    {
        // Arrange
        _repo.ExistsByNameAsync(TenantId, "OAuth2Svc", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateExternalServiceCommand(
            TenantId, "OAuth2Svc", null, "RestApi", "https://oauth.api.com",
            null, null, null, "OAuth2",
            new[]
            {
                new AuthConfigInput("client_id", "cid"),
                new AuthConfigInput("client_secret", "csecret"),
                new AuthConfigInput("token_url", "https://auth.example.com/token")
            },
            UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.AuthType.Should().Be(AuthType.OAuth2);
        await _repo.Received(1).ReplaceAuthConfigsAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<ServiceAuthConfig>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_09_04_Returns_409_Duplicate_Name()
    {
        // Arrange
        _repo.ExistsByNameAsync(TenantId, "DuplicateName", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateExternalServiceCommand(
            TenantId, "DuplicateName", null, "RestApi", "https://api.com",
            null, null, null, "None", null, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_DUPLICATE_SERVICE_NAME");
    }

    [Fact]
    public async Task TP_CFG_09_05_Returns_422_Invalid_Auth_Config()
    {
        // Arrange
        _repo.ExistsByNameAsync(TenantId, "Svc", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // api_key requires header_name + api_key, but only providing header_name
        var command = new CreateExternalServiceCommand(
            TenantId, "Svc", null, "RestApi", "https://api.com",
            null, null, null, "ApiKey",
            new[] { new AuthConfigInput("header_name", "X-Api-Key") },
            UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_INVALID_AUTH_CONFIG");
    }

    [Fact]
    public async Task TP_CFG_09_06_Returns_422_Missing_Required_Fields()
    {
        // Arrange
        _repo.ExistsByNameAsync(TenantId, "Svc", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // bearer_token requires "token", but providing nothing
        var command = new CreateExternalServiceCommand(
            TenantId, "Svc", null, "RestApi", "https://api.com",
            null, null, null, "BearerToken",
            null,
            UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_INVALID_AUTH_CONFIG");
    }

    [Fact]
    public async Task TP_CFG_09_08_Credentials_Encrypted_With_AES()
    {
        // Arrange
        _repo.ExistsByNameAsync(TenantId, "EncSvc", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateExternalServiceCommand(
            TenantId, "EncSvc", null, "RestApi", "https://api.com",
            null, null, null, "BearerToken",
            new[] { new AuthConfigInput("token", "my-secret-token") },
            UserId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _encryption.Received(1).Encrypt("my-secret-token");

        await _repo.Received(1).ReplaceAuthConfigsAsync(
            Arg.Any<Guid>(),
            Arg.Is<IEnumerable<ServiceAuthConfig>>(configs =>
                configs.Any(c => c.ValueEncrypted == "encrypted_my-secret-token")),
            Arg.Any<CancellationToken>());
    }
}
