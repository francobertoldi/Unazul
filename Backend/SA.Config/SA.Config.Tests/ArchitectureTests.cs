using FluentAssertions;
using Xunit;

namespace SA.Config.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Reference_Application()
    {
        var domainAssembly = typeof(Config.Domain.Entities.ParameterGroup).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        referencedAssemblies
            .Should()
            .NotContain(a => a.Name == "SA.Config.Application",
                "Domain layer must not depend on Application layer");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var domainAssembly = typeof(Config.Domain.Entities.ParameterGroup).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        referencedAssemblies
            .Should()
            .NotContain(a => a.Name == "SA.Config.Infrastructure",
                "Domain layer must not depend on Infrastructure layer");
    }
}
