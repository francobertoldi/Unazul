using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Organization.Application.Queries.Branches;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Organization.Tests.Unit.Queries.Branches;

public sealed class ListBranchesByEntityQueryHandlerTests
{
    private readonly IEntityRepository _entityRepo = Substitute.For<IEntityRepository>();
    private readonly IBranchRepository _branchRepo = Substitute.For<IBranchRepository>();
    private readonly ListBranchesByEntityQueryHandler _sut;

    public ListBranchesByEntityQueryHandlerTests()
    {
        _sut = new ListBranchesByEntityQueryHandler(_entityRepo, _branchRepo);
    }

    private static Entity CreateEntity(Guid? tenantId = null)
    {
        return Entity.Create(
            tenantId ?? Guid.NewGuid(), "Test Entity", "20-12345678-1",
            EntityType.Bank, EntityStatus.Active,
            null, null, null, null, null, null, null);
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
    public async Task TP_ORG_10_01_Returns_Branches_Mapped_To_BranchDto()
    {
        // Arrange
        var entity = CreateEntity();
        var branch1 = CreateBranch(entityId: entity.Id, tenantId: entity.TenantId, code: "BR-001");
        var branch2 = CreateBranch(entityId: entity.Id, tenantId: entity.TenantId, code: "BR-002");

        var query = new ListBranchesByEntityQuery(entity.Id);

        _entityRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);
        _branchRepo.ListByEntityAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Branch> { branch1, branch2 });

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(branch1.Id);
        result[0].Code.Should().Be("BR-001");
        result[1].Id.Should().Be(branch2.Id);
        result[1].Code.Should().Be("BR-002");
    }

    [Fact]
    public async Task TP_ORG_10_08_Entity_Not_Found_Throws_ORG_ENTITY_NOT_FOUND()
    {
        // Arrange
        var query = new ListBranchesByEntityQuery(Guid.NewGuid());

        _entityRepo.GetByIdAsync(query.EntityId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_ENTITY_NOT_FOUND");
    }

    [Fact]
    public async Task Empty_List_Returns_Empty_Result()
    {
        // Arrange
        var entity = CreateEntity();
        var query = new ListBranchesByEntityQuery(entity.Id);

        _entityRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);
        _branchRepo.ListByEntityAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Branch>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
