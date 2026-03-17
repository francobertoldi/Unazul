using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Commands.Tenants;
using SA.Organization.Application.Interfaces;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Events;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Tenants;

public sealed class DeleteTenantCommandHandlerTests
{
    private readonly ITenantRepository _repo = Substitute.For<ITenantRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly DeleteTenantCommandHandler _sut;

    public DeleteTenantCommandHandlerTests()
    {
        _sut = new DeleteTenantCommandHandler(_repo, _eventPublisher);
    }

    private static Tenant CreateTenant(
        string name = "Test Org",
        string identifier = "20-12345678-1") =>
        Tenant.Create(name, identifier, TenantStatus.Active, null, null, null, null, null, null, null, null);

    [Fact]
    public async Task TP_ORG_05_01_Successful_Deletion_Calls_DeleteAsync_SaveChanges_And_PublishAsync()
    {
        // Arrange
        var tenant = CreateTenant();
        var deletedBy = Guid.NewGuid();
        var command = new DeleteTenantCommand(tenant.Id, deletedBy);

        _repo.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);
        _repo.CountEntitiesAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _repo.Received(1).DeleteAsync(tenant, Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<TenantDeletedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_05_02_Has_Entities_Throws_ORG_TENANT_HAS_ENTITIES()
    {
        // Arrange
        var tenant = CreateTenant();
        var command = new DeleteTenantCommand(tenant.Id, Guid.NewGuid());

        _repo.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);
        _repo.CountEntitiesAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(3);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_TENANT_HAS_ENTITIES");
    }

    [Fact]
    public async Task TP_ORG_05_03_Not_Found_Throws_ORG_TENANT_NOT_FOUND()
    {
        // Arrange
        var command = new DeleteTenantCommand(Guid.NewGuid(), Guid.NewGuid());

        _repo.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_TENANT_NOT_FOUND");
    }

    [Fact]
    public async Task TP_ORG_05_06_Verify_TenantDeletedEvent_Published_With_Correct_Data()
    {
        // Arrange
        var tenant = CreateTenant("My Org");
        var deletedBy = Guid.NewGuid();
        var command = new DeleteTenantCommand(tenant.Id, deletedBy);

        _repo.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(tenant);
        _repo.CountEntitiesAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<TenantDeletedEvent>(e =>
                e.TenantId == tenant.Id &&
                e.TenantName == "My Org" &&
                e.DeletedBy == deletedBy &&
                e.CorrelationId != Guid.Empty),
            Arg.Any<CancellationToken>());
    }
}
