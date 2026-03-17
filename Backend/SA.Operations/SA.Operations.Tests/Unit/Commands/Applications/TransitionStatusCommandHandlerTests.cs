using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Operations.Application.Commands.Applications;
using SA.Operations.Application.Events;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;

namespace SA.Operations.Tests.Unit.Commands.Applications;

public sealed class TransitionStatusCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IBeneficiaryRepository _beneficiaryRepository = Substitute.For<IBeneficiaryRepository>();
    private readonly ITraceEventRepository _traceEventRepository = Substitute.For<ITraceEventRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly TransitionStatusCommandHandler _sut;

    public TransitionStatusCommandHandlerTests()
    {
        _sut = new TransitionStatusCommandHandler(
            _applicationRepository,
            _beneficiaryRepository,
            _traceEventRepository,
            _eventPublisher);
    }

    private static SA.Operations.Domain.Entities.Application CreateAppWithStatus(Guid tenantId, ApplicationStatus status)
    {
        var app = SA.Operations.Domain.Entities.Application.Create(
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SOL-2026-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product",
            "Plan",
            Guid.NewGuid());

        if (status != ApplicationStatus.Draft)
            typeof(SA.Operations.Domain.Entities.Application).GetProperty("Status")!.SetValue(app, status);

        return app;
    }

    private TransitionStatusCommand BuildCommand(
        Guid? applicationId = null,
        Guid? tenantId = null,
        string newStatus = "Pending",
        string action = "transition",
        string? detail = null)
    {
        return new TransitionStatusCommand(
            ApplicationId: applicationId ?? Guid.NewGuid(),
            TenantId: tenantId ?? Guid.NewGuid(),
            NewStatus: newStatus,
            Action: action,
            Detail: detail,
            UserId: Guid.NewGuid(),
            UserName: "Admin User");
    }

    [Fact(DisplayName = "TP_OPS_05_01 - Transitions Draft to Pending with beneficiaries sum 100")]
    public async Task TP_OPS_05_01_TransitionsDraftToPending()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId, newStatus: "Pending");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _beneficiaryRepository.SumPercentageAsync(app.Id, Arg.Any<CancellationToken>()).Returns(100m);
        _applicationRepository.TransitionStatusAsync(
            app.Id, ApplicationStatus.Draft, ApplicationStatus.Pending, command.UserId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(app.Id);
        result.Code.Should().Be("SOL-2026-001");
        result.Status.Should().Be("Pending");
    }

    [Fact(DisplayName = "TP_OPS_05_02 - Transitions Pending to InReview")]
    public async Task TP_OPS_05_02_TransitionsPendingToInReview()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Pending);
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId, newStatus: "InReview");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _applicationRepository.TransitionStatusAsync(
            app.Id, ApplicationStatus.Pending, ApplicationStatus.InReview, command.UserId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("InReview");
    }

    [Fact(DisplayName = "TP_OPS_05_03 - Throws OPS_INVALID_STATUS for invalid status string")]
    public async Task TP_OPS_05_03_ThrowsForInvalidStatus()
    {
        // Arrange
        var command = BuildCommand(newStatus: "INVALID_STATUS");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_INVALID_STATUS");
    }

    [Fact(DisplayName = "TP_OPS_05_04 - Throws OPS_SETTLED_VIA_SETTLEMENT_ONLY for Settled status")]
    public async Task TP_OPS_05_04_ThrowsForSettledStatus()
    {
        // Arrange
        var command = BuildCommand(newStatus: "Settled");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_SETTLED_VIA_SETTLEMENT_ONLY");
    }

    [Fact(DisplayName = "TP_OPS_05_05 - Throws OPS_APPLICATION_NOT_FOUND for missing application")]
    public async Task TP_OPS_05_05_ThrowsForMissingApplication()
    {
        // Arrange
        var command = BuildCommand(newStatus: "Pending");
        _applicationRepository.GetByIdAsync(command.ApplicationId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICATION_NOT_FOUND");
    }

    [Fact(DisplayName = "TP_OPS_05_06 - Throws OPS_APPLICATION_NOT_FOUND for wrong tenant")]
    public async Task TP_OPS_05_06_ThrowsForWrongTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var wrongTenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var command = BuildCommand(applicationId: app.Id, tenantId: wrongTenantId, newStatus: "Pending");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICATION_NOT_FOUND");
    }

    [Fact(DisplayName = "TP_OPS_05_07 - Throws OPS_BENEFICIARY_SUM_NOT_100 when sum is not 100")]
    public async Task TP_OPS_05_07_ThrowsWhenBeneficiarySumNot100()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId, newStatus: "Pending");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _beneficiaryRepository.SumPercentageAsync(app.Id, Arg.Any<CancellationToken>()).Returns(80m);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_BENEFICIARY_SUM_NOT_100");
    }

    [Fact(DisplayName = "TP_OPS_05_08 - Throws OPS_TRANSITION_CONFLICT when affected rows is 0")]
    public async Task TP_OPS_05_08_ThrowsForTransitionConflict()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId, newStatus: "Pending");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _beneficiaryRepository.SumPercentageAsync(app.Id, Arg.Any<CancellationToken>()).Returns(100m);
        _applicationRepository.TransitionStatusAsync(
            app.Id, ApplicationStatus.Draft, ApplicationStatus.Pending, command.UserId, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_TRANSITION_CONFLICT");
    }

    [Fact(DisplayName = "TP_OPS_05_09 - Publishes ApplicationStatusChangedEvent on success")]
    public async Task TP_OPS_05_09_PublishesStatusChangedEvent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Pending);
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId, newStatus: "InReview");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _applicationRepository.TransitionStatusAsync(
            app.Id, ApplicationStatus.Pending, ApplicationStatus.InReview, command.UserId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<ApplicationStatusChangedEvent>(e =>
                e.ApplicationId == app.Id &&
                e.OldStatus == "Pending" &&
                e.NewStatus == "InReview" &&
                e.TenantId == tenantId),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "TP_OPS_05_10 - Creates trace event with detail")]
    public async Task TP_OPS_05_10_CreatesTraceEventWithDetail()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Pending);
        var command = BuildCommand(
            applicationId: app.Id,
            tenantId: tenantId,
            newStatus: "InReview",
            action: "approve",
            detail: "All documents verified");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _applicationRepository.TransitionStatusAsync(
            app.Id, ApplicationStatus.Pending, ApplicationStatus.InReview, command.UserId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _traceEventRepository.Received(1).AddAsync(
            Arg.Is<TraceEvent>(t =>
                t.ApplicationId == app.Id &&
                t.TenantId == tenantId &&
                t.State == "InReview" &&
                t.Action == "approve"),
            Arg.Any<CancellationToken>());
        await _traceEventRepository.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
