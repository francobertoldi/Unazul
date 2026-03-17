using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Coverages;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Coverages;

public sealed class UpdateCoverageCommandHandlerTests
{
    private readonly ICoverageRepository _coverageRepository = Substitute.For<ICoverageRepository>();
    private readonly UpdateCoverageCommandHandler _sut;

    public UpdateCoverageCommandHandlerTests()
    {
        _sut = new UpdateCoverageCommandHandler(_coverageRepository);
    }

    [Fact]
    public async Task TP_CAT_06_02_Update_SumInsuredAndPremium_Succeeds()
    {
        // Arrange
        var coverage = Coverage.Create(Guid.NewGuid(), Guid.NewGuid(), "Life", "life", 100000m, 500m, 30);
        var command = new UpdateCoverageCommand(Guid.NewGuid(), Guid.NewGuid(), 200000m, 750m, 60);

        _coverageRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(coverage);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _coverageRepository.Received(1).Update(coverage);
        await _coverageRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CoverageNotFound_ThrowsCAT_COVERAGE_NOT_FOUND()
    {
        // Arrange
        var command = new UpdateCoverageCommand(Guid.NewGuid(), Guid.NewGuid(), 200000m, 750m, 60);

        _coverageRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_COVERAGE_NOT_FOUND");
    }
}
