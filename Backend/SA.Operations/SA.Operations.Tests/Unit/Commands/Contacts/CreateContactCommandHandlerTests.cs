using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Operations.Application.Commands.Contacts;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;

namespace SA.Operations.Tests.Unit.Commands.Contacts;

public sealed class CreateContactCommandHandlerTests
{
    private readonly IApplicantRepository _applicantRepository = Substitute.For<IApplicantRepository>();
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly CreateContactCommandHandler _sut;

    public CreateContactCommandHandlerTests()
    {
        _sut = new CreateContactCommandHandler(_applicantRepository, _contactRepository);
    }

    private static Applicant CreateApplicant(Guid tenantId)
    {
        return Applicant.Create(tenantId, "John", "Doe", DocumentType.DNI, "12345678", null, null, null);
    }

    [Fact]
    public async Task TP_OPS_08_01_CreateContact_Successfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicant = CreateApplicant(tenantId);
        var command = new CreateContactCommand(applicant.Id, tenantId, "Personal", "john@test.com", null, null);

        _applicantRepository.GetByIdAsync(applicant.Id, Arg.Any<CancellationToken>())
            .Returns(applicant);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Type.Should().Be("Personal");

        await _contactRepository.Received(1).AddAsync(Arg.Any<ApplicantContact>(), Arg.Any<CancellationToken>());
        await _contactRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_OPS_08_02_ThrowsOpsApplicantNotFound_WhenApplicantMissing()
    {
        // Arrange
        var command = new CreateContactCommand(Guid.NewGuid(), Guid.NewGuid(), "Personal", "john@test.com", null, null);

        _applicantRepository.GetByIdAsync(command.ApplicantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICANT_NOT_FOUND");
    }

    [Fact]
    public async Task TP_OPS_08_03_ThrowsOpsApplicantNotFound_WhenWrongTenant()
    {
        // Arrange
        var applicant = CreateApplicant(Guid.NewGuid());
        var differentTenantId = Guid.NewGuid();
        var command = new CreateContactCommand(applicant.Id, differentTenantId, "Personal", "john@test.com", null, null);

        _applicantRepository.GetByIdAsync(applicant.Id, Arg.Any<CancellationToken>())
            .Returns(applicant);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICANT_NOT_FOUND");
    }

    [Fact]
    public async Task TP_OPS_08_04_ThrowsOpsInvalidContactType_WhenTypeInvalid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicant = CreateApplicant(tenantId);
        var command = new CreateContactCommand(applicant.Id, tenantId, "InvalidType", "john@test.com", null, null);

        _applicantRepository.GetByIdAsync(applicant.Id, Arg.Any<CancellationToken>())
            .Returns(applicant);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_INVALID_CONTACT_TYPE");
    }
}
