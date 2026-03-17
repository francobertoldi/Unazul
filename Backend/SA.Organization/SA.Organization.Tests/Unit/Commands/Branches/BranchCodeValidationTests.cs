using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Commands.Branches;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Branches;

public sealed class BranchCodeValidationTests
{
    private readonly IEntityRepository _entityRepository = Substitute.For<IEntityRepository>();
    private readonly IBranchRepository _branchRepository = Substitute.For<IBranchRepository>();
    private readonly CreateBranchCommandHandler _createSut;
    private readonly UpdateBranchCommandHandler _updateSut;

    public BranchCodeValidationTests()
    {
        _createSut = new CreateBranchCommandHandler(_entityRepository, _branchRepository);
        _updateSut = new UpdateBranchCommandHandler(_branchRepository);
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task TP_ORG_10_04_Create_Branch_Returns_BranchDto_With_AllFields()
    {
        var entity = CreateEntity();
        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _branchRepository.ExistsByCodeAsync(entity.TenantId, "BR-NEW", Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateBranchCommand(entity.Id, "New Branch", "BR-NEW",
            "123 St", "City", "Province", "1000", "AR", "+54111234", "br@test.com", true);

        var result = await _createSut.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Branch");
        result.Code.Should().Be("BR-NEW");
        result.Address.Should().Be("123 St");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task TP_ORG_10_02b_Update_Branch_Changes_All_Fields()
    {
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var branch = Branch.Create(entityId, tenantId, "Old Name", "BR-001",
            "Old Addr", "Old City", null, null, null, null, null, true);

        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);

        var command = new UpdateBranchCommand(entityId, branch.Id, "New Name",
            "New Addr", "New City", "New Prov", "2000", "AR", "+54119999", "new@test.com", false);

        var result = await _updateSut.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Address.Should().Be("New Addr");
        result.City.Should().Be("New City");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task TP_ORG_10_05_Update_Branch_Saves_Changes()
    {
        var entityId = Guid.NewGuid();
        var branch = Branch.Create(entityId, Guid.NewGuid(), "Test", "BR-001",
            null, null, null, null, null, null, null, true);

        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);

        var command = new UpdateBranchCommand(entityId, branch.Id, "Updated",
            null, null, null, null, null, null, null, true);

        await _updateSut.Handle(command, CancellationToken.None);

        _branchRepository.Received(1).Update(branch);
        await _branchRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_ORG_10_07b_Create_Branch_With_Existing_Code_In_Same_Tenant_Throws()
    {
        var tenantId = Guid.NewGuid();
        var entity = CreateEntity(tenantId);
        _entityRepository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _branchRepository.ExistsByCodeAsync(tenantId, "BR-001", Arg.Any<CancellationToken>()).Returns(true);

        var command = new CreateBranchCommand(entity.Id, "Branch", "BR-001",
            null, null, null, null, null, null, null, true);

        Func<Task> act = async () => await _createSut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_DUPLICATE_BRANCH_CODE");
    }

    [Fact]
    public async Task TP_ORG_10_10b_Update_Branch_WrongEntity_Throws()
    {
        var correctEntityId = Guid.NewGuid();
        var wrongEntityId = Guid.NewGuid();
        var branch = Branch.Create(correctEntityId, Guid.NewGuid(), "Test", "BR-001",
            null, null, null, null, null, null, null, true);

        _branchRepository.GetByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);

        var command = new UpdateBranchCommand(wrongEntityId, branch.Id, "Updated",
            null, null, null, null, null, null, null, true);

        Func<Task> act = async () => await _updateSut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_BRANCH_NOT_IN_ENTITY");
    }

    [Fact]
    public async Task TP_ORG_10_09b_Update_Branch_NotFound_Throws()
    {
        _branchRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var command = new UpdateBranchCommand(Guid.NewGuid(), Guid.NewGuid(), "Updated",
            null, null, null, null, null, null, null, true);

        Func<Task> act = async () => await _updateSut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_BRANCH_NOT_FOUND");
    }
}
