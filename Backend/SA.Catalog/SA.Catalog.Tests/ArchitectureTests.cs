using System.Reflection;
using FluentAssertions;
using Xunit;

namespace SA.Catalog.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void DomainShouldNotReferenceApplication()
    {
        var domainAssembly = typeof(SA.Catalog.Domain.Entities.ProductFamily).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Catalog.Application");
    }

    [Fact]
    public void DomainShouldNotReferenceDataAccess()
    {
        var domainAssembly = typeof(SA.Catalog.Domain.Entities.ProductFamily).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Catalog.DataAccess.EntityFramework");
    }

    [Fact]
    public void ApplicationShouldNotReferenceDataAccessImplementation()
    {
        var appAssembly = typeof(SA.Catalog.Application.Interfaces.IEntityValidationService).Assembly;
        var referencedAssemblies = appAssembly.GetReferencedAssemblies();
        referencedAssemblies.Should().NotContain(a => a.Name == "SA.Catalog.DataAccess.EntityFramework");
    }

    [Fact]
    public void AllDomainEntitiesShouldBeSealed()
    {
        var domainAssembly = typeof(SA.Catalog.Domain.Entities.ProductFamily).Assembly;
        var entities = domainAssembly.GetTypes()
            .Where(t => t.Namespace != null
                && t.Namespace.Contains("Entities")
                && t.IsClass && !t.IsAbstract);
        foreach (var entity in entities)
        {
            entity.IsSealed.Should().BeTrue($"{entity.Name} should be sealed");
        }
    }

    [Fact]
    public void AllCommandHandlersShouldBeSealed()
    {
        var appAssembly = typeof(SA.Catalog.Application.Interfaces.IEntityValidationService).Assembly;
        var handlers = appAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("CommandHandler") && t.IsClass);
        foreach (var handler in handlers)
        {
            handler.IsSealed.Should().BeTrue($"{handler.Name} should be sealed");
        }
    }
}
