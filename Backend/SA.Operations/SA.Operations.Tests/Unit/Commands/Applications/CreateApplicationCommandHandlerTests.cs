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

public sealed class CreateApplicationCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IApplicantRepository _applicantRepository = Substitute.For<IApplicantRepository>();
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IAddressRepository _addressRepository = Substitute.For<IAddressRepository>();
    private readonly IBeneficiaryRepository _beneficiaryRepository = Substitute.For<IBeneficiaryRepository>();
    private readonly ITraceEventRepository _traceEventRepository = Substitute.For<ITraceEventRepository>();
    private readonly ICatalogServiceClient _catalogClient = Substitute.For<ICatalogServiceClient>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly CreateApplicationCommandHandler _sut;

    public CreateApplicationCommandHandlerTests()
    {
        _sut = new CreateApplicationCommandHandler(
            _applicationRepository,
            _applicantRepository,
            _contactRepository,
            _addressRepository,
            _beneficiaryRepository,
            _traceEventRepository,
            _catalogClient,
            _eventPublisher);
    }

    private CreateApplicationCommand BuildCommand(
        string documentType = "DNI",
        CreateContactInput[]? contacts = null,
        CreateAddressInput[]? addresses = null,
        CreateBeneficiaryInput[]? beneficiaries = null)
    {
        return new CreateApplicationCommand(
            TenantId: Guid.NewGuid(),
            EntityId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            PlanId: Guid.NewGuid(),
            FirstName: "Juan",
            LastName: "Perez",
            DocumentType: documentType,
            DocumentNumber: "12345678",
            BirthDate: new DateOnly(1990, 1, 1),
            Gender: "Male",
            Occupation: "Engineer",
            Contacts: contacts,
            Addresses: addresses,
            Beneficiaries: beneficiaries,
            CreatedBy: Guid.NewGuid(),
            CreatedByName: "Admin User");
    }

    private void SetupValidCatalog(Guid productId, Guid planId, bool isActive = true)
    {
        _catalogClient.ValidateProductAndPlanAsync(productId, planId, Arg.Any<CancellationToken>())
            .Returns(new CatalogProductResult(productId, "Product A", planId, "Plan A", isActive));
    }

    private void SetupNewApplicant()
    {
        _applicantRepository.GetByDocumentAsync(
            Arg.Any<Guid>(), Arg.Any<DocumentType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsNull();
    }

    private Applicant SetupExistingApplicant(Guid tenantId)
    {
        var applicant = Applicant.Create(tenantId, "Existing", "User", DocumentType.DNI, "12345678", null, null, null);
        _applicantRepository.GetByDocumentAsync(
            tenantId, Arg.Any<DocumentType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(applicant);
        return applicant;
    }

    [Fact(DisplayName = "TP_OPS_01_01 - Creates application with valid data returning id, code SOL-YYYY-NNN and status Draft")]
    public async Task TP_OPS_01_01_CreatesApplicationWithValidData()
    {
        // Arrange
        var command = BuildCommand();
        SetupValidCatalog(command.ProductId, command.PlanId);
        SetupNewApplicant();
        _applicationRepository.GetNextSequenceAsync(command.TenantId, Arg.Any<CancellationToken>())
            .Returns(7);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Code.Should().MatchRegex(@"^SOL-\d{4}-\d{3}$");
        result.Code.Should().Contain($"SOL-{DateTime.UtcNow.Year}-007");
        result.Status.Should().Be("Draft");
    }

    [Fact(DisplayName = "TP_OPS_01_02 - Creates with new applicant calling AddAsync on applicantRepository")]
    public async Task TP_OPS_01_02_CreatesWithNewApplicant()
    {
        // Arrange
        var command = BuildCommand();
        SetupValidCatalog(command.ProductId, command.PlanId);
        SetupNewApplicant();
        _applicationRepository.GetNextSequenceAsync(command.TenantId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _applicantRepository.Received(1).AddAsync(Arg.Any<Applicant>(), Arg.Any<CancellationToken>());
        _applicantRepository.DidNotReceive().Update(Arg.Any<Applicant>());
    }

    [Fact(DisplayName = "TP_OPS_01_03 - Creates with existing applicant calling Update on applicantRepository")]
    public async Task TP_OPS_01_03_CreatesWithExistingApplicant()
    {
        // Arrange
        var command = BuildCommand();
        SetupValidCatalog(command.ProductId, command.PlanId);
        SetupExistingApplicant(command.TenantId);
        _applicationRepository.GetNextSequenceAsync(command.TenantId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _applicantRepository.Received(1).Update(Arg.Any<Applicant>());
        await _applicantRepository.DidNotReceive().AddAsync(Arg.Any<Applicant>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "TP_OPS_01_04 - Creates with contacts and addresses persisting them")]
    public async Task TP_OPS_01_04_CreatesWithContactsAndAddresses()
    {
        // Arrange
        var contacts = new[]
        {
            new CreateContactInput("Personal", "juan@test.com", "+54", "1122334455")
        };
        var addresses = new[]
        {
            new CreateAddressInput("Home", "Av. Corrientes", "1234", null, null, "CABA", "Buenos Aires", "1043", null, null)
        };
        var command = BuildCommand(contacts: contacts, addresses: addresses);
        SetupValidCatalog(command.ProductId, command.PlanId);
        SetupNewApplicant();
        _applicationRepository.GetNextSequenceAsync(command.TenantId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _contactRepository.Received(1).AddAsync(Arg.Any<ApplicantContact>(), Arg.Any<CancellationToken>());
        await _contactRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _addressRepository.Received(1).AddAsync(Arg.Any<ApplicantAddress>(), Arg.Any<CancellationToken>());
        await _addressRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "TP_OPS_01_05 - Creates with beneficiaries persisting them")]
    public async Task TP_OPS_01_05_CreatesWithBeneficiaries()
    {
        // Arrange
        var beneficiaries = new[]
        {
            new CreateBeneficiaryInput("Maria", "Perez", "Spouse", 60m),
            new CreateBeneficiaryInput("Carlos", "Perez", "Child", 40m)
        };
        var command = BuildCommand(beneficiaries: beneficiaries);
        SetupValidCatalog(command.ProductId, command.PlanId);
        SetupNewApplicant();
        _applicationRepository.GetNextSequenceAsync(command.TenantId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _beneficiaryRepository.Received(2).AddAsync(Arg.Any<Beneficiary>(), Arg.Any<CancellationToken>());
        await _beneficiaryRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "TP_OPS_01_06 - Publishes ApplicationCreatedEvent on success")]
    public async Task TP_OPS_01_06_PublishesApplicationCreatedEvent()
    {
        // Arrange
        var command = BuildCommand();
        SetupValidCatalog(command.ProductId, command.PlanId);
        SetupNewApplicant();
        _applicationRepository.GetNextSequenceAsync(command.TenantId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<ApplicationCreatedEvent>(e =>
                e.TenantId == command.TenantId &&
                e.EntityId == command.EntityId &&
                e.ProductId == command.ProductId),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "TP_OPS_01_07 - Throws OPS_PRODUCT_PLAN_NOT_FOUND when catalog returns null")]
    public async Task TP_OPS_01_07_ThrowsWhenCatalogReturnsNull()
    {
        // Arrange
        var command = BuildCommand();
        _catalogClient.ValidateProductAndPlanAsync(command.ProductId, command.PlanId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_PRODUCT_PLAN_NOT_FOUND");
    }

    [Fact(DisplayName = "TP_OPS_01_08 - Throws OPS_PRODUCT_PLAN_INACTIVE when product is inactive")]
    public async Task TP_OPS_01_08_ThrowsWhenProductInactive()
    {
        // Arrange
        var command = BuildCommand();
        SetupValidCatalog(command.ProductId, command.PlanId, isActive: false);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_PRODUCT_PLAN_INACTIVE");
    }

    [Fact(DisplayName = "TP_OPS_01_09 - Throws OPS_INVALID_DOCUMENT_TYPE for bad document type")]
    public async Task TP_OPS_01_09_ThrowsForInvalidDocumentType()
    {
        // Arrange
        var command = BuildCommand(documentType: "INVALID_DOC");
        SetupValidCatalog(command.ProductId, command.PlanId);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_INVALID_DOCUMENT_TYPE");
    }

    [Fact(DisplayName = "TP_OPS_01_10 - Throws OPS_INVALID_CONTACT_TYPE for bad contact type")]
    public async Task TP_OPS_01_10_ThrowsForInvalidContactType()
    {
        // Arrange
        var contacts = new[]
        {
            new CreateContactInput("INVALID_CONTACT", "test@test.com", null, null)
        };
        var command = BuildCommand(contacts: contacts);
        SetupValidCatalog(command.ProductId, command.PlanId);
        SetupNewApplicant();
        _applicationRepository.GetNextSequenceAsync(command.TenantId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_INVALID_CONTACT_TYPE");
    }

    [Fact(DisplayName = "TP_OPS_01_11 - Throws OPS_INVALID_ADDRESS_TYPE for bad address type")]
    public async Task TP_OPS_01_11_ThrowsForInvalidAddressType()
    {
        // Arrange
        var addresses = new[]
        {
            new CreateAddressInput("INVALID_ADDR", "Street", "123", null, null, "City", "Province", "1000", null, null)
        };
        var command = BuildCommand(addresses: addresses);
        SetupValidCatalog(command.ProductId, command.PlanId);
        SetupNewApplicant();
        _applicationRepository.GetNextSequenceAsync(command.TenantId, Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_INVALID_ADDRESS_TYPE");
    }
}
