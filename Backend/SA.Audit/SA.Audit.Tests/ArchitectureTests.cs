using System.Reflection;
using FluentAssertions;
using SA.Audit.Domain.Entities;
using Xunit;

namespace SA.Audit.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void DomainShouldNotReferenceApplicationOrInfrastructureOrDataAccessOrApi()
    {
        var domainAssembly = typeof(AuditLog).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Audit.Application");
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Audit.Infrastructure");
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Audit.DataAccess.EntityFramework");
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Audit.Api");
    }

    [Fact]
    public void AllCommandHandlersShouldBeSealed()
    {
        var appAssembly = typeof(SA.Audit.Application.Commands.IngestDomainEventCommandHandler).Assembly;
        var handlers = appAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("CommandHandler") && t.IsClass);

        handlers.Should().NotBeEmpty();
        foreach (var handler in handlers)
        {
            handler.IsSealed.Should().BeTrue($"{handler.Name} should be sealed");
        }
    }

    [Fact]
    public void AllQueryHandlersShouldBeSealed()
    {
        var appAssembly = typeof(SA.Audit.Application.Commands.IngestDomainEventCommandHandler).Assembly;
        var handlers = appAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("QueryHandler") && t.IsClass);

        handlers.Should().NotBeEmpty();
        foreach (var handler in handlers)
        {
            handler.IsSealed.Should().BeTrue($"{handler.Name} should be sealed");
        }
    }

    [Fact]
    public void AuditLogEntityShouldBeSealed()
    {
        typeof(AuditLog).IsSealed.Should().BeTrue("AuditLog entity should be sealed");
    }

    [Fact]
    public void AuditLogShouldNotHavePublicUpdateMethod()
    {
        var methods = typeof(AuditLog).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().NotContain(m => m.Name == "Update",
            "AuditLog is append-only and should not have a public Update method");
    }
}
