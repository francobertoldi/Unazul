using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Plans;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Plans;

public sealed class UpdateProductPlanCommandHandlerTests
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
    private readonly UpdateProductPlanCommandHandler _sut;

    public UpdateProductPlanCommandHandlerTests()
    {
        _sut = new UpdateProductPlanCommandHandler(
            _productRepository, _familyRepository, _planRepository,
            _commissionPlanRepository, _loanAttributesRepository,
            _insuranceAttributesRepository, _accountAttributesRepository,
            _cardAttributesRepository, _investmentAttributesRepository,
            _coverageRepository);
    }

    [Fact]
    public async Task TP_CAT_05_06_Update_Plan_Succeeds()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var plan = ProductPlan.Create(productId, tenantId, "Plan", "PL001", 100m, "ARS", 12, null);
        var product = Product.Create(tenantId, Guid.NewGuid(), familyId,
            "Test", "PROD001", null, ProductStatus.Active,
            new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var family = ProductFamily.Create(tenantId, "PREST001", "Loans", Guid.NewGuid());

        _planRepository.GetByIdAsync(planId, Arg.Any<CancellationToken>()).Returns(plan);
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>()).Returns(family);

        var command = new UpdateProductPlanCommand(
            tenantId, planId, productId,
            "Updated Plan", "PL001", 200m, "USD",
            24, null,
            new LoanAttributesInput("French", 30.0m, null, null),
            null, null, null, null, null, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _planRepository.Received(1).Update(plan);
        await _planRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_05_12_Update_PlanNotFound_ThrowsCAT_PLAN_NOT_FOUND()
    {
        // Arrange
        var command = new UpdateProductPlanCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Plan", "PL001", 100m, "ARS",
            null, null, null, null, null, null, null, null, Guid.NewGuid());

        _planRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PLAN_NOT_FOUND");
    }

    [Fact]
    public async Task Update_ProductDeprecated_ThrowsCAT_PRODUCT_DEPRECATED()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var plan = ProductPlan.Create(productId, tenantId, "Plan", "PL001", 100m, "ARS", null, null);
        var product = Product.Create(tenantId, Guid.NewGuid(), familyId,
            "Test", "PROD001", null, ProductStatus.Active,
            new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        product.Deprecate(Guid.NewGuid());

        _planRepository.GetByIdAsync(planId, Arg.Any<CancellationToken>()).Returns(plan);
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        var command = new UpdateProductPlanCommand(
            tenantId, planId, productId,
            "Updated", "PL001", 100m, "ARS",
            null, null, null, null, null, null, null, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_DEPRECATED");
    }
}
