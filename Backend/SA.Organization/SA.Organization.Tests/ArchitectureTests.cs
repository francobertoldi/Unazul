using System.Reflection;
using FluentAssertions;
using Xunit;

namespace SA.Organization.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void DomainShouldNotReferenceApplication()
    {
        var domainAssembly = typeof(SA.Organization.Domain.Entities.Tenant).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Organization.Application");
    }

    [Fact]
    public void DomainShouldNotReferenceDataAccess()
    {
        var domainAssembly = typeof(SA.Organization.Domain.Entities.Tenant).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Organization.DataAccess.EntityFramework");
    }

    [Fact]
    public void ApplicationShouldNotReferenceDataAccessImplementation()
    {
        var appAssembly = typeof(SA.Organization.Application.Interfaces.IIntegrationEventPublisher).Assembly;
        var referencedAssemblies = appAssembly.GetReferencedAssemblies();
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Organization.DataAccess.EntityFramework");
    }

    [Fact]
    public void AllCommandHandlersShouldBeSealed()
    {
        var appAssembly = typeof(SA.Organization.Application.Interfaces.IIntegrationEventPublisher).Assembly;
        var handlers = appAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("CommandHandler") && t.IsClass);
        foreach (var handler in handlers)
        {
            handler.IsSealed.Should().BeTrue($"{handler.Name} should be sealed");
        }
    }

    [Fact]
    public void AllQueryHandlersShouldBeSealed()
    {
        var appAssembly = typeof(SA.Organization.Application.Interfaces.IIntegrationEventPublisher).Assembly;
        var handlers = appAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("QueryHandler") && t.IsClass);
        foreach (var handler in handlers)
        {
            handler.IsSealed.Should().BeTrue($"{handler.Name} should be sealed");
        }
    }
}
