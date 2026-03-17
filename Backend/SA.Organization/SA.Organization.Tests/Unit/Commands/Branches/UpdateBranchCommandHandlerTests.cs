using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Commands.Branches;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Branches;

public sealed class UpdateBranchCommandHandlerTests
{
    private readonly IBranchRepository _branchRepo = Substitute.For<IBranchRepository>();
    private readonly UpdateBranchCommandHandler _sut;

    public UpdateBranchCommandHandlerTests()
    {
        _sut = new UpdateBranchCommandHandler(_branchRepo);
    }

    private static Branch CreateBranch(Guid? entityId = null, Guid? tenantId = null, string code = "BR-001")
    {
        return Branch.Create(
            entityId ?? Guid.NewGuid(),
            tenantId ?? Guid.NewGuid(),
            "Test Branch",
            code,
            null, null, null, null, null, null, null, true);
    }

    [Fact]
    public async Task TP_ORG_10_02_Successful_Update_Returns_BranchDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var branch = CreateBranch(entityId: entityId);

        var command = new UpdateBranchCommand(
            entityId,
            branch.Id,
            "Updated Branch",
            "New Address",
            "Rosario",
            "Santa Fe",
            "2000",
            "AR",
            "+54 341 999-0000",
            "updated@test.com",
            false);

        _branchRepo.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>())
            .Returns(branch);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(branch.Id);
        result.EntityId.Should().Be(entityId);
        result.Name.Should().Be("Updated Branch");
        result.Address.Should().Be("New Address");
        result.City.Should().Be("Rosario");
        result.Province.Should().Be("Santa Fe");
        result.IsActive.Should().BeFalse();

        _branchRepo.Received(1).Update(branch);
        await _branchRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_10_09_Branch_Not_Found_Throws_ORG_BRANCH_NOT_FOUND()
    {
        // Arrange
        var command = new UpdateBranchCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Name",
            null, null, null, null, null, null, null,
            true);

        _branchRepo.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_BRANCH_NOT_FOUND");
    }

    [Fact]
    public async Task TP_ORG_10_10_Branch_Not_In_Entity_Throws_ORG_BRANCH_NOT_IN_ENTITY()
    {
        // Arrange
        var branch = CreateBranch(entityId: Guid.NewGuid());
        var differentEntityId = Guid.NewGuid();

        var command = new UpdateBranchCommand(
            differentEntityId,
            branch.Id,
            "Name",
            null, null, null, null, null, null, null,
            true);

        _branchRepo.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>())
            .Returns(branch);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_BRANCH_NOT_IN_ENTITY");
    }
}
