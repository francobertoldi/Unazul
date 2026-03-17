using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Commands.Branches;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Branches;

public sealed class CreateBranchCommandHandlerTests
{
    private readonly IEntityRepository _entityRepo = Substitute.For<IEntityRepository>();
    private readonly IBranchRepository _branchRepo = Substitute.For<IBranchRepository>();
    private readonly CreateBranchCommandHandler _sut;

    public CreateBranchCommandHandlerTests()
    {
        _sut = new CreateBranchCommandHandler(_entityRepo, _branchRepo);
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
    }

    private static CreateBranchCommand CreateValidCommand(Guid? entityId = null) =>
        new(
            entityId ?? Guid.NewGuid(),
            "Sucursal Centro",
            "SC-001",
            "Av. Corrientes 1234",
            "Buenos Aires",
            "CABA",
            "1000",
            "AR",
            "+54 11 5555-1234",
            "centro@test.com",
            true);

    [Fact]
    public async Task TP_ORG_10_01_Successful_Creation_Returns_BranchDto()
    {
        // Arrange
        var entity = CreateEntity();
        var command = CreateValidCommand(entity.Id);

        _entityRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);
        _branchRepo.ExistsByCodeAsync(entity.TenantId, command.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.EntityId.Should().Be(entity.Id);
        result.Name.Should().Be("Sucursal Centro");
        result.Code.Should().Be("SC-001");
        result.Address.Should().Be("Av. Corrientes 1234");
        result.City.Should().Be("Buenos Aires");
        result.IsActive.Should().BeTrue();

        await _branchRepo.Received(1).AddAsync(Arg.Any<Branch>(), Arg.Any<CancellationToken>());
        await _branchRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_10_07_Duplicate_Code_Throws_ORG_DUPLICATE_BRANCH_CODE()
    {
        // Arrange
        var entity = CreateEntity();
        var command = CreateValidCommand(entity.Id);

        _entityRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);
        _branchRepo.ExistsByCodeAsync(entity.TenantId, command.Code, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_DUPLICATE_BRANCH_CODE");
    }

    [Fact]
    public async Task TP_ORG_10_08_Entity_Not_Found_Throws_ORG_ENTITY_NOT_FOUND()
    {
        // Arrange
        var command = CreateValidCommand();

        _entityRepo.GetByIdAsync(command.EntityId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_ENTITY_NOT_FOUND");
    }

    [Fact]
    public async Task TP_ORG_10_16_TenantId_Inherited_From_Entity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entity = CreateEntity(tenantId);
        var command = CreateValidCommand(entity.Id);

        _entityRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);
        _branchRepo.ExistsByCodeAsync(entity.TenantId, command.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _branchRepo.Received(1).AddAsync(
            Arg.Is<Branch>(b => b.TenantId == tenantId),
            Arg.Any<CancellationToken>());
    }
}
