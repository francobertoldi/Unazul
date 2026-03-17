using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Plans;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Plans;

public sealed class DeleteProductPlanCommandHandlerTests
{
    private readonly IProductPlanRepository _planRepository = Substitute.For<IProductPlanRepository>();
    private readonly ICoverageRepository _coverageRepository = Substitute.For<ICoverageRepository>();
    private readonly IPlanLoanAttributesRepository _loanAttributesRepository = Substitute.For<IPlanLoanAttributesRepository>();
    private readonly IPlanInsuranceAttributesRepository _insuranceAttributesRepository = Substitute.For<IPlanInsuranceAttributesRepository>();
    private readonly IPlanAccountAttributesRepository _accountAttributesRepository = Substitute.For<IPlanAccountAttributesRepository>();
    private readonly IPlanCardAttributesRepository _cardAttributesRepository = Substitute.For<IPlanCardAttributesRepository>();
    private readonly IPlanInvestmentAttributesRepository _investmentAttributesRepository = Substitute.For<IPlanInvestmentAttributesRepository>();
    private readonly DeleteProductPlanCommandHandler _sut;

    public DeleteProductPlanCommandHandlerTests()
    {
        _sut = new DeleteProductPlanCommandHandler(
            _planRepository, _coverageRepository,
            _loanAttributesRepository, _insuranceAttributesRepository,
            _accountAttributesRepository, _cardAttributesRepository,
            _investmentAttributesRepository);
    }

    [Fact]
    public async Task TP_CAT_05_07_Delete_Plan_SucceedsWithCascade()
    {
        // Arrange
        var plan = ProductPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan", "PL001", 100m, "ARS", null, null);
        var command = new DeleteProductPlanCommand(Guid.NewGuid(), Guid.NewGuid());

        _planRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(plan);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _planRepository.Received(1).Delete(plan);
        await _planRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _coverageRepository.Received(1).DeleteByPlanIdAsync(plan.Id, Arg.Any<CancellationToken>());
        await _loanAttributesRepository.Received(1).DeleteByPlanIdAsync(plan.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_PlanNotFound_ThrowsCAT_PLAN_NOT_FOUND()
    {
        // Arrange
        var command = new DeleteProductPlanCommand(Guid.NewGuid(), Guid.NewGuid());

        _planRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PLAN_NOT_FOUND");
    }
}
