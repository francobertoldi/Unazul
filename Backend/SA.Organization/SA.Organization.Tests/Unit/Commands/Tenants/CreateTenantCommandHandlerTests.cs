using FluentAssertions;
using NSubstitute;
using SA.Organization.Application.Commands.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Tenants;

public sealed class CreateTenantCommandHandlerTests
{
    private readonly ITenantRepository _repo = Substitute.For<ITenantRepository>();
    private readonly CreateTenantCommandHandler _sut;

    public CreateTenantCommandHandlerTests()
    {
        _sut = new CreateTenantCommandHandler(_repo);
    }

    private static CreateTenantCommand CreateValidCommand(
        string name = "Test Org",
        string identifier = "20-12345678-1",
        string status = "Active") =>
        new(name, identifier, status, "Calle 1", "Buenos Aires", "CABA", "1000", "AR", "+54 11 1234", "test@org.com", null);

    [Fact]
    public async Task TP_ORG_02_01_Successful_Creation_Returns_TenantDto_With_Correct_Fields()
    {
        // Arrange
        var command = CreateValidCommand();
        _repo.ExistsByIdentifierAsync(command.Identifier, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Org");
        result.Identifier.Should().Be("20-12345678-1");
        result.Status.Should().Be("Active");
        result.Address.Should().Be("Calle 1");
        result.City.Should().Be("Buenos Aires");
        result.Province.Should().Be("CABA");
        result.Phone.Should().Be("+54 11 1234");
        result.Email.Should().Be("test@org.com");
    }

    [Fact]
    public async Task TP_ORG_02_03_Duplicate_Identifier_Throws_ORG_DUPLICATE_IDENTIFIER()
    {
        // Arrange
        var command = CreateValidCommand();
        _repo.ExistsByIdentifierAsync(command.Identifier, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_DUPLICATE_IDENTIFIER");
    }

    [Fact]
    public async Task TP_ORG_02_06_Invalid_Status_Throws_ORG_INVALID_STATUS()
    {
        // Arrange
        var command = CreateValidCommand(status: "NonExistentStatus");

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_STATUS");
    }

    [Fact]
    public async Task TP_ORG_02_04_Verify_AddAsync_And_SaveChangesAsync_Called()
    {
        // Arrange
        var command = CreateValidCommand();
        _repo.ExistsByIdentifierAsync(command.Identifier, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _repo.Received(1).AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
