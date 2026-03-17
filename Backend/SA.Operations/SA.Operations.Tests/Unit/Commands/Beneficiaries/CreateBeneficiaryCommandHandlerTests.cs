using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Operations.Application.Commands.Beneficiaries;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;
using AppEntity = SA.Operations.Domain.Entities.Application;

namespace SA.Operations.Tests.Unit.Commands.Beneficiaries;

public sealed class CreateBeneficiaryCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IBeneficiaryRepository _beneficiaryRepository = Substitute.For<IBeneficiaryRepository>();
    private readonly CreateBeneficiaryCommandHandler _sut;

    public CreateBeneficiaryCommandHandlerTests()
    {
        _sut = new CreateBeneficiaryCommandHandler(_applicationRepository, _beneficiaryRepository);
    }

    private static AppEntity CreateApplication(Guid tenantId)
    {
        return AppEntity.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), "OPS-001", Guid.NewGuid(), Guid.NewGuid(), "Product", "Plan", Guid.NewGuid());
    }

    [Fact]
    public async Task TP_OPS_09_01_CreateBeneficiary_Successfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var command = new CreateBeneficiaryCommand(app.Id, tenantId, "Jane", "Smith", "Spouse", 50m);

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(app);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Percentage.Should().Be(50m);

        await _beneficiaryRepository.Received(1).AddAsync(Arg.Any<Beneficiary>(), Arg.Any<CancellationToken>());
        await _beneficiaryRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_OPS_09_02_ThrowsOpsApplicationNotFound_WhenMissing()
    {
        // Arrange
        var command = new CreateBeneficiaryCommand(Guid.NewGuid(), Guid.NewGuid(), "Jane", "Smith", "Spouse", 50m);

        _applicationRepository.GetByIdAsync(command.ApplicationId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICATION_NOT_FOUND");
    }

    [Fact]
    public async Task TP_OPS_09_03_ReturnsCorrectFullName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var command = new CreateBeneficiaryCommand(app.Id, tenantId, "Maria", "Garcia", "Daughter", 25m);

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(app);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.FullName.Should().Be("Maria Garcia");
    }
}
