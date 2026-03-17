using FluentAssertions;
using Xunit;

namespace SA.Operations.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Reference_Application()
    {
        var domainAssembly = typeof(Operations.Domain.Entities.Application).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        referencedAssemblies
            .Should()
            .NotContain(a => a.Name == "SA.Operations.Application",
                "Domain layer must not depend on Application layer");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var domainAssembly = typeof(Operations.Domain.Entities.Application).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        referencedAssemblies
            .Should()
            .NotContain(a => a.Name == "SA.Operations.Infrastructure",
                "Domain layer must not depend on Infrastructure layer");
    }
}
