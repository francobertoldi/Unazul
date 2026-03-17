using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Operations.Application.Commands.Observations;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;
using AppEntity = SA.Operations.Domain.Entities.Application;

namespace SA.Operations.Tests.Unit.Commands.Observations;

public sealed class CreateObservationCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IObservationRepository _observationRepository = Substitute.For<IObservationRepository>();
    private readonly CreateObservationCommandHandler _sut;

    public CreateObservationCommandHandlerTests()
    {
        _sut = new CreateObservationCommandHandler(_applicationRepository, _observationRepository);
    }

    private static AppEntity CreateApplication(Guid tenantId)
    {
        return AppEntity.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), "OPS-001", Guid.NewGuid(), Guid.NewGuid(), "Product", "Plan", Guid.NewGuid());
    }

    [Fact]
    public async Task TP_OPS_12_01_CreateObservation_Successfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var userId = Guid.NewGuid();
        var command = new CreateObservationCommand(app.Id, tenantId, "This is an observation", userId, "Admin User");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(app);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Content.Should().Be("This is an observation");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        await _observationRepository.Received(1).AddAsync(Arg.Any<ApplicationObservation>(), Arg.Any<CancellationToken>());
        await _observationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_OPS_12_02_ThrowsOpsApplicationNotFound_WhenMissing()
    {
        // Arrange
        var command = new CreateObservationCommand(Guid.NewGuid(), Guid.NewGuid(), "Observation", Guid.NewGuid(), "Admin User");

        _applicationRepository.GetByIdAsync(command.ApplicationId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICATION_NOT_FOUND");
    }
}
