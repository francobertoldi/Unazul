using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.ExternalServices;
using SA.Config.Application.Dtos.ExternalServices;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.ExternalServices;

public sealed class UpdateExternalServiceCommandHandlerTests
{
    private readonly IExternalServiceRepository _repo = Substitute.For<IExternalServiceRepository>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly UpdateExternalServiceCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public UpdateExternalServiceCommandHandlerTests()
    {
        _sut = new UpdateExternalServiceCommandHandler(_repo, _encryption, _eventPublisher);
        _encryption.Encrypt(Arg.Any<string>()).Returns(x => "encrypted_" + x.Arg<string>());
    }

    private static ExternalService CreateService(string name = "OriginalName")
    {
        return ExternalService.Create(
            TenantId, name, "desc", ServiceType.RestApi, "https://api.example.com",
            ServiceStatus.Active, 30000, 3, AuthType.ApiKey, Guid.NewGuid());
    }

    [Fact]
    public async Task TP_CFG_10_01_Updates_Name_Successfully()
    {
        // Arrange
        var service = CreateService("OldName");
        _repo.GetByIdWithAuthConfigsAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);
        _repo.ExistsByNameAsync(TenantId, "NewName", service.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdateExternalServiceCommand(
            service.Id, "NewName", null, null, null, null, null, null, null, null, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("NewName");
        _repo.Received(1).Update(service);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_10_02_Keeps_Credentials_When_Not_Provided()
    {
        // Arrange
        var service = CreateService();
        service.AuthConfigs.Add(ServiceAuthConfig.Create(service.Id, "header_name", "encrypted_X-Api-Key"));
        service.AuthConfigs.Add(ServiceAuthConfig.Create(service.Id, "api_key", "encrypted_secret"));

        _repo.GetByIdWithAuthConfigsAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);

        // Only update description, no auth changes
        var command = new UpdateExternalServiceCommand(
            service.Id, null, "new description", null, null, null, null, null, null, null, UserId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert - should not replace auth configs
        await _repo.DidNotReceive().ReplaceAuthConfigsAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<ServiceAuthConfig>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_10_03_Changes_AuthType_Replaces_Credentials()
    {
        // Arrange
        var service = CreateService();
        service.AuthConfigs.Add(ServiceAuthConfig.Create(service.Id, "header_name", "encrypted_X-Api-Key"));
        service.AuthConfigs.Add(ServiceAuthConfig.Create(service.Id, "api_key", "encrypted_secret"));

        _repo.GetByIdWithAuthConfigsAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);

        // Change from api_key to bearer_token
        var command = new UpdateExternalServiceCommand(
            service.Id, null, null, null, null, null, null, null, "BearerToken",
            new[] { new AuthConfigInput("token", "new-bearer-token") },
            UserId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert - should replace auth configs with new ones
        await _repo.Received(1).ReplaceAuthConfigsAsync(
            service.Id,
            Arg.Is<IEnumerable<ServiceAuthConfig>>(configs =>
                configs.Any(c => c.Key == "token" && c.ValueEncrypted == "encrypted_new-bearer-token")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_10_04_Returns_404_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repo.GetByIdWithAuthConfigsAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new UpdateExternalServiceCommand(
            nonExistentId, "Name", null, null, null, null, null, null, null, null, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_SERVICE_NOT_FOUND");
    }

    [Fact]
    public async Task TP_CFG_10_05_Returns_409_Duplicate_Name()
    {
        // Arrange
        var service = CreateService("OldName");
        _repo.GetByIdWithAuthConfigsAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);
        _repo.ExistsByNameAsync(TenantId, "TakenName", service.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdateExternalServiceCommand(
            service.Id, "TakenName", null, null, null, null, null, null, null, null, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_DUPLICATE_SERVICE_NAME");
    }
}
