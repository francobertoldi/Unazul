using System.Net;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.ExternalServices;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.ExternalServices;

public sealed class TestExternalServiceCommandHandlerTests
{
    private readonly IExternalServiceRepository _repo = Substitute.For<IExternalServiceRepository>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly TestExternalServiceCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public TestExternalServiceCommandHandlerTests()
    {
        _sut = new TestExternalServiceCommandHandler(_repo, _encryption, _eventPublisher, _httpClientFactory);
        _encryption.Decrypt(Arg.Any<string>()).Returns(x => ((string)x[0]).Replace("encrypted_", ""));
    }

    private static ExternalService CreateServiceWithAuth(
        ServiceStatus status = ServiceStatus.Active,
        AuthType authType = AuthType.ApiKey)
    {
        var service = ExternalService.Create(
            TenantId, "TestSvc", "desc", ServiceType.RestApi, "https://api.example.com",
            status, 30000, 3, authType, Guid.NewGuid());

        if (authType == AuthType.ApiKey)
        {
            service.AuthConfigs.Add(ServiceAuthConfig.Create(service.Id, "header_name", "encrypted_X-Api-Key"));
            service.AuthConfigs.Add(ServiceAuthConfig.Create(service.Id, "api_key", "encrypted_my-key"));
        }

        return service;
    }

    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode)
    {
        var handler = new FakeHttpMessageHandler(statusCode);
        return new HttpClient(handler);
    }

    [Fact]
    public async Task TP_CFG_11_01_Successful_Test_Returns_Success_True()
    {
        // Arrange
        var service = CreateServiceWithAuth();
        _repo.GetByIdWithAuthConfigsAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);
        _httpClientFactory.CreateClient().Returns(CreateMockHttpClient(HttpStatusCode.OK));

        // Act
        var result = await _sut.Handle(new TestExternalServiceCommand(service.Id), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        result.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task TP_CFG_11_02_Updates_LastTestedAt()
    {
        // Arrange
        var service = CreateServiceWithAuth();
        service.LastTestedAt.Should().BeNull();

        _repo.GetByIdWithAuthConfigsAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);
        _httpClientFactory.CreateClient().Returns(CreateMockHttpClient(HttpStatusCode.OK));

        // Act
        await _sut.Handle(new TestExternalServiceCommand(service.Id), CancellationToken.None);

        // Assert
        service.LastTestedAt.Should().NotBeNull();
        service.LastTestSuccess.Should().BeTrue();
        _repo.Received(1).Update(service);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_11_04_Failed_Test_Returns_Success_False()
    {
        // Arrange
        var service = CreateServiceWithAuth();
        _repo.GetByIdWithAuthConfigsAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);
        _httpClientFactory.CreateClient().Returns(CreateMockHttpClient(HttpStatusCode.InternalServerError));

        // Act
        var result = await _sut.Handle(new TestExternalServiceCommand(service.Id), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("500");
    }

    [Fact]
    public async Task TP_CFG_11_07_Returns_404_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repo.GetByIdWithAuthConfigsAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new TestExternalServiceCommand(nonExistentId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_SERVICE_NOT_FOUND");
    }

    /// <summary>
    /// Fake HttpMessageHandler for testing HTTP calls without real network access.
    /// </summary>
    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
