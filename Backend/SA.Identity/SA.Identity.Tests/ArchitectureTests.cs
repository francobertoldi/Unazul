using FluentAssertions;
using Xunit;

namespace SA.Identity.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Reference_Application()
    {
        var domainAssembly = typeof(Domain.Entities.User).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        referencedAssemblies
            .Should()
            .NotContain(a => a.Name == "SA.Identity.Application",
                "Domain layer must not depend on Application layer");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var domainAssembly = typeof(Domain.Entities.User).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        referencedAssemblies
            .Should()
            .NotContain(a => a.Name == "SA.Identity.Infrastructure",
                "Domain layer must not depend on Infrastructure layer");
    }

    [Fact]
    public void User_Create_Should_Set_Properties()
    {
        var tenantId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        var user = Domain.Entities.User.Create(
            tenantId,
            "testuser",
            "hashedpassword",
            "test@example.com",
            "Test",
            "User",
            null,
            null,
            createdBy);

        user.Id.Should().NotBeEmpty();
        user.TenantId.Should().Be(tenantId);
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.IsActive.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void User_RecordFailedLogin_Should_Lock_After_Five_Attempts()
    {
        var user = Domain.Entities.User.Create(
            Guid.NewGuid(), "test", "hash", "t@t.com", "A", "B", null, null, Guid.NewGuid());

        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        user.IsLocked.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(5);
    }

    [Fact]
    public void User_RecordSuccessfulLogin_Should_Reset_Counter()
    {
        var user = Domain.Entities.User.Create(
            Guid.NewGuid(), "test", "hash", "t@t.com", "A", "B", null, null, Guid.NewGuid());

        user.RecordFailedLogin();
        user.RecordFailedLogin();
        user.RecordSuccessfulLogin();

        user.FailedLoginAttempts.Should().Be(0);
        user.LastLogin.Should().NotBeNull();
    }

    [Fact]
    public void RefreshToken_Revoke_Should_Set_Revoked()
    {
        var token = Domain.Entities.RefreshToken.Create(
            Guid.NewGuid(),
            "tokenhash",
            DateTime.UtcNow.AddDays(7));

        token.IsValid.Should().BeTrue();

        token.Revoke();

        token.Revoked.Should().BeTrue();
        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Role_Create_Should_Not_Be_System()
    {
        var role = Domain.Entities.Role.Create(
            Guid.NewGuid(), "TestRole", "Description", Guid.NewGuid());

        role.IsSystem.Should().BeFalse();
        role.Name.Should().Be("TestRole");
    }
}
