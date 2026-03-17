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

public sealed class UpdateApplicationCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IApplicantRepository _applicantRepository = Substitute.For<IApplicantRepository>();
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IAddressRepository _addressRepository = Substitute.For<IAddressRepository>();
    private readonly IBeneficiaryRepository _beneficiaryRepository = Substitute.For<IBeneficiaryRepository>();
    private readonly ICatalogServiceClient _catalogClient = Substitute.For<ICatalogServiceClient>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly UpdateApplicationCommandHandler _sut;

    public UpdateApplicationCommandHandlerTests()
    {
        _sut = new UpdateApplicationCommandHandler(
            _applicationRepository,
            _applicantRepository,
            _contactRepository,
            _addressRepository,
            _beneficiaryRepository,
            _catalogClient,
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

    private UpdateApplicationCommand BuildCommand(
        Guid? applicationId = null,
        Guid? tenantId = null,
        Guid? productId = null,
        Guid? planId = null,
        string? firstName = null,
        CreateContactInput[]? contacts = null,
        CreateAddressInput[]? addresses = null,
        CreateBeneficiaryInput[]? beneficiaries = null)
    {
        return new UpdateApplicationCommand(
            ApplicationId: applicationId ?? Guid.NewGuid(),
            TenantId: tenantId ?? Guid.NewGuid(),
            EntityId: null,
            ProductId: productId,
            PlanId: planId,
            FirstName: firstName,
            LastName: null,
            BirthDate: null,
            Gender: null,
            Occupation: null,
            Contacts: contacts,
            Addresses: addresses,
            Beneficiaries: beneficiaries,
            UpdatedBy: Guid.NewGuid());
    }

    [Fact(DisplayName = "TP_OPS_02_01 - Updates draft application successfully")]
    public async Task TP_OPS_02_01_UpdatesDraftApplicationSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId, firstName: "UpdatedName");

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        var applicant = Applicant.Create(tenantId, "Juan", "Perez", DocumentType.DNI, "12345678", null, null, null);
        _applicantRepository.GetByIdAsync(app.ApplicantId, Arg.Any<CancellationToken>()).Returns(applicant);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(app.Id);
        result.Code.Should().Be("SOL-2026-001");
        result.Status.Should().Be("Draft");
        _applicationRepository.Received(1).Update(app);
        await _applicationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "TP_OPS_02_02 - Throws OPS_APPLICATION_NOT_FOUND for missing application")]
    public async Task TP_OPS_02_02_ThrowsForMissingApplication()
    {
        // Arrange
        var command = BuildCommand();
        _applicationRepository.GetByIdAsync(command.ApplicationId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICATION_NOT_FOUND");
    }

    [Fact(DisplayName = "TP_OPS_02_03 - Throws OPS_ONLY_DRAFT_EDITABLE for non-draft application")]
    public async Task TP_OPS_02_03_ThrowsForNonDraftApplication()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Pending);
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId);

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_ONLY_DRAFT_EDITABLE");
    }

    [Fact(DisplayName = "TP_OPS_02_04 - Validates product/plan when changed throwing OPS_PRODUCT_PLAN_NOT_FOUND")]
    public async Task TP_OPS_02_04_ThrowsWhenProductPlanNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var newProductId = Guid.NewGuid();
        var newPlanId = Guid.NewGuid();
        var command = BuildCommand(
            applicationId: app.Id,
            tenantId: tenantId,
            productId: newProductId,
            planId: newPlanId);

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _catalogClient.ValidateProductAndPlanAsync(newProductId, newPlanId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_PRODUCT_PLAN_NOT_FOUND");
    }

    [Fact(DisplayName = "TP_OPS_02_05 - Replaces contacts when provided")]
    public async Task TP_OPS_02_05_ReplacesContactsWhenProvided()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var contacts = new[]
        {
            new CreateContactInput("Personal", "new@test.com", "+54", "1155667788")
        };
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId, firstName: "Juan", contacts: contacts);

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        var applicant = Applicant.Create(tenantId, "Juan", "Perez", DocumentType.DNI, "12345678", null, null, null);
        _applicantRepository.GetByIdAsync(app.ApplicantId, Arg.Any<CancellationToken>()).Returns(applicant);

        var existingContact = ApplicantContact.Create(applicant.Id, tenantId, ContactType.Work, "old@test.com", null, null);
        _contactRepository.GetByApplicantIdAsync(applicant.Id, Arg.Any<CancellationToken>())
            .Returns(new List<ApplicantContact> { existingContact });

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _contactRepository.Received(1).Delete(existingContact);
        await _contactRepository.Received(1).AddAsync(Arg.Any<ApplicantContact>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "TP_OPS_02_06 - Replaces beneficiaries when provided")]
    public async Task TP_OPS_02_06_ReplacesBeneficiariesWhenProvided()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var beneficiaries = new[]
        {
            new CreateBeneficiaryInput("Ana", "Lopez", "Spouse", 100m)
        };
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId, beneficiaries: beneficiaries);

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _applicantRepository.GetByIdAsync(app.ApplicantId, Arg.Any<CancellationToken>()).ReturnsNull();

        var existingBeneficiary = Beneficiary.Create(app.Id, tenantId, "Old", "Beneficiary", "Child", 50m);
        _beneficiaryRepository.GetByApplicationIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Beneficiary> { existingBeneficiary });

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _beneficiaryRepository.Received(1).Delete(existingBeneficiary);
        await _beneficiaryRepository.Received(1).AddAsync(Arg.Any<Beneficiary>(), Arg.Any<CancellationToken>());
        await _beneficiaryRepository.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "TP_OPS_02_07 - Publishes event on success")]
    public async Task TP_OPS_02_07_PublishesEventOnSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateAppWithStatus(tenantId, ApplicationStatus.Draft);
        var command = BuildCommand(applicationId: app.Id, tenantId: tenantId);

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _applicantRepository.GetByIdAsync(app.ApplicantId, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<ApplicationStatusChangedEvent>(e =>
                e.ApplicationId == app.Id &&
                e.OldStatus == "Draft" &&
                e.NewStatus == "Draft" &&
                e.TenantId == tenantId),
            Arg.Any<CancellationToken>());
    }
}
