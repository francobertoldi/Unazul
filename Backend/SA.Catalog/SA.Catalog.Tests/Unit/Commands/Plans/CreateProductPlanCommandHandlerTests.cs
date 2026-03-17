using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Plans;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Plans;

public sealed class CreateProductPlanCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IProductFamilyRepository _familyRepository = Substitute.For<IProductFamilyRepository>();
    private readonly IProductPlanRepository _planRepository = Substitute.For<IProductPlanRepository>();
    private readonly ICommissionPlanRepository _commissionPlanRepository = Substitute.For<ICommissionPlanRepository>();
    private readonly IPlanLoanAttributesRepository _loanAttributesRepository = Substitute.For<IPlanLoanAttributesRepository>();
    private readonly IPlanInsuranceAttributesRepository _insuranceAttributesRepository = Substitute.For<IPlanInsuranceAttributesRepository>();
    private readonly IPlanAccountAttributesRepository _accountAttributesRepository = Substitute.For<IPlanAccountAttributesRepository>();
    private readonly IPlanCardAttributesRepository _cardAttributesRepository = Substitute.For<IPlanCardAttributesRepository>();
    private readonly IPlanInvestmentAttributesRepository _investmentAttributesRepository = Substitute.For<IPlanInvestmentAttributesRepository>();
    private readonly ICoverageRepository _coverageRepository = Substitute.For<ICoverageRepository>();
    private readonly CreateProductPlanCommandHandler _sut;

    public CreateProductPlanCommandHandlerTests()
    {
        _sut = new CreateProductPlanCommandHandler(
            _productRepository, _familyRepository, _planRepository,
            _commissionPlanRepository, _loanAttributesRepository,
            _insuranceAttributesRepository, _accountAttributesRepository,
            _cardAttributesRepository, _investmentAttributesRepository,
            _coverageRepository);
    }

    private void SetupProductAndFamily(Guid productId, Guid familyId, Guid tenantId, string familyCode)
    {
        var product = Product.Create(tenantId, Guid.NewGuid(), familyId,
            "Test", "PROD001", null, ProductStatus.Active,
            new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var family = ProductFamily.Create(tenantId, familyCode, "Description", Guid.NewGuid());

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>()).Returns(family);
    }

    [Fact]
    public async Task TP_CAT_05_01_Create_LoanPlan_WithCorrectAttributes_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupProductAndFamily(productId, familyId, tenantId, "PREST001");

        var command = new CreateProductPlanCommand(
            tenantId, productId,
            "Loan Plan", "LP001", 1000m, "ARS",
            12, null,
            new LoanAttributesInput("French", 25.5m, 30.0m, 500m),
            null, null, null, null, null, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _planRepository.Received(1).AddAsync(Arg.Any<ProductPlan>(), Arg.Any<CancellationToken>());
        await _loanAttributesRepository.Received(1).AddAsync(Arg.Any<PlanLoanAttributes>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_05_02_Create_InsurancePlan_WithCoverages_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupProductAndFamily(productId, familyId, tenantId, "SEG001");

        var coverages = new[]
        {
            new CoverageInput("Life Coverage", "life", 100000m, 500m, 30)
        };

        var command = new CreateProductPlanCommand(
            tenantId, productId,
            "Insurance Plan", "IP001", 500m, "ARS",
            null, null,
            null,
            new InsuranceAttributesInput(500m, 100000m, 30, "life"),
            null, null, null, coverages, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _insuranceAttributesRepository.Received(1).AddAsync(Arg.Any<PlanInsuranceAttributes>(), Arg.Any<CancellationToken>());
        await _coverageRepository.Received(1).AddRangeAsync(Arg.Any<List<Coverage>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_05_03_Create_AccountPlan_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupProductAndFamily(productId, familyId, tenantId, "CTA001");

        var command = new CreateProductPlanCommand(
            tenantId, productId,
            "Account Plan", "AP001", 0m, "ARS",
            null, null,
            null, null,
            new AccountAttributesInput(100m, 5000m, 2.5m, "savings"),
            null, null, null, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _accountAttributesRepository.Received(1).AddAsync(Arg.Any<PlanAccountAttributes>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_05_04_Create_CardPlan_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupProductAndFamily(productId, familyId, tenantId, "TARJETA001");

        var command = new CreateProductPlanCommand(
            tenantId, productId,
            "Card Plan", "CP001", 1500m, "ARS",
            null, null,
            null, null, null,
            new CardAttributesInput(500000m, 3000m, 45.0m, "Visa", "Gold"),
            null, null, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _cardAttributesRepository.Received(1).AddAsync(Arg.Any<PlanCardAttributes>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_05_05_Create_InvestmentPlan_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupProductAndFamily(productId, familyId, tenantId, "INV001");

        var command = new CreateProductPlanCommand(
            tenantId, productId,
            "Investment Plan", "VP001", 0m, "USD",
            null, null,
            null, null, null, null,
            new InvestmentAttributesInput(10000m, 8.5m, 365, "Medium"),
            null, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _investmentAttributesRepository.Received(1).AddAsync(Arg.Any<PlanInvestmentAttributes>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_05_08_Create_CategoryMismatch_ThrowsCAT_CATEGORY_MISMATCH()
    {
        // Arrange — loan family but providing insurance attributes
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        SetupProductAndFamily(productId, familyId, tenantId, "PREST001");

        var command = new CreateProductPlanCommand(
            tenantId, productId,
            "Bad Plan", "BP001", 100m, "ARS",
            null, null,
            null,
            new InsuranceAttributesInput(500m, 100000m, 30, "life"),
            null, null, null, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_CATEGORY_MISMATCH");
    }

    [Fact]
    public async Task TP_CAT_05_09_Create_ProductDeprecated_ThrowsCAT_PRODUCT_DEPRECATED()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        var product = Product.Create(tenantId, Guid.NewGuid(), familyId,
            "Test", "PROD001", null, ProductStatus.Active,
            new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        product.Deprecate(Guid.NewGuid());

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        var command = new CreateProductPlanCommand(
            tenantId, productId,
            "Plan", "PL001", 100m, "ARS",
            null, null,
            new LoanAttributesInput("French", 25.5m, null, null),
            null, null, null, null, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_DEPRECATED");
    }

    [Fact]
    public async Task TP_CAT_05_10_Create_CommissionPlanNotFound_ThrowsCAT_COMMISSION_PLAN_NOT_FOUND()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var commissionId = Guid.NewGuid();
        SetupProductAndFamily(productId, familyId, tenantId, "PREST001");

        _commissionPlanRepository.GetByIdAsync(commissionId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new CreateProductPlanCommand(
            tenantId, productId,
            "Plan", "PL001", 100m, "ARS",
            12, commissionId,
            new LoanAttributesInput("French", 25.5m, null, null),
            null, null, null, null, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_COMMISSION_PLAN_NOT_FOUND");
    }
}
