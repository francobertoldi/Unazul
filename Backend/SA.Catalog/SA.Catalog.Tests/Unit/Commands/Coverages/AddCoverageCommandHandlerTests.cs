using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Coverages;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Coverages;

public sealed class AddCoverageCommandHandlerTests
{
    private readonly IProductPlanRepository _planRepository = Substitute.For<IProductPlanRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IProductFamilyRepository _familyRepository = Substitute.For<IProductFamilyRepository>();
    private readonly ICoverageRepository _coverageRepository = Substitute.For<ICoverageRepository>();
    private readonly AddCoverageCommandHandler _sut;

    public AddCoverageCommandHandlerTests()
    {
        _sut = new AddCoverageCommandHandler(_planRepository, _productRepository, _familyRepository, _coverageRepository);
    }

    private void SetupPlanProductFamily(Guid planId, Guid productId, Guid familyId, Guid tenantId, string familyCode)
    {
        var plan = ProductPlan.Create(productId, tenantId, "Plan", "PL001", 100m, "ARS", null, null);
        var product = Product.Create(tenantId, Guid.NewGuid(), familyId,
            "Test", "PROD001", null, ProductStatus.Active,
            new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var family = ProductFamily.Create(tenantId, familyCode, "Description", Guid.NewGuid());

        _planRepository.GetByIdAsync(planId, Arg.Any<CancellationToken>()).Returns(plan);
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>()).Returns(family);
    }

    [Fact]
    public async Task TP_CAT_06_01_Add_CoverageToInsurancePlan_Succeeds()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupPlanProductFamily(planId, productId, familyId, tenantId, "SEG001");

        _coverageRepository.ExistsByNameAsync(planId, "Life", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new AddCoverageCommand(tenantId, planId, "Life", "life", 100000m, 500m, 30, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _coverageRepository.Received(1).AddAsync(Arg.Any<Coverage>(), Arg.Any<CancellationToken>());
        await _coverageRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_06_05_Add_DuplicateCoverageName_ThrowsCAT_DUPLICATE_COVERAGE_NAME()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupPlanProductFamily(planId, productId, familyId, tenantId, "SEG001");

        _coverageRepository.ExistsByNameAsync(planId, "Life", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new AddCoverageCommand(tenantId, planId, "Life", "life", 100000m, 500m, 30, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_DUPLICATE_COVERAGE_NAME");
    }

    [Fact]
    public async Task TP_CAT_06_06_Add_NonInsuranceProduct_ThrowsCAT_COVERAGE_NOT_INSURANCE()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupPlanProductFamily(planId, productId, familyId, tenantId, "PREST001");

        var command = new AddCoverageCommand(tenantId, planId, "Life", "life", 100000m, 500m, 30, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_COVERAGE_NOT_INSURANCE");
    }

    [Fact]
    public async Task TP_CAT_06_07_Add_SumInsuredZero_ThrowsCAT_INVALID_SUM_INSURED()
    {
        // Arrange
        var command = new AddCoverageCommand(Guid.NewGuid(), Guid.NewGuid(), "Life", "life", 0m, 500m, 30, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_INVALID_SUM_INSURED");
    }

    [Fact]
    public async Task TP_CAT_06_08_Add_PlanNotFound_ThrowsCAT_PLAN_NOT_FOUND()
    {
        // Arrange
        var command = new AddCoverageCommand(Guid.NewGuid(), Guid.NewGuid(), "Life", "life", 100000m, 500m, 30, Guid.NewGuid());

        _planRepository.GetByIdAsync(command.PlanId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PLAN_NOT_FOUND");
    }
}
