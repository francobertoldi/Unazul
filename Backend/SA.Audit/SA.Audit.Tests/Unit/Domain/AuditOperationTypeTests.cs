using FluentAssertions;
using SA.Audit.Domain;
using Xunit;

namespace SA.Audit.Tests.Unit.Domain;

public sealed class AuditOperationTypeTests
{
    [Theory]
    [InlineData("Crear")]
    [InlineData("Editar")]
    [InlineData("Eliminar")]
    [InlineData("Login")]
    [InlineData("Logout")]
    [InlineData("CambiarContrasena")]
    [InlineData("CambiarEstado")]
    [InlineData("Liquidar")]
    [InlineData("Exportar")]
    [InlineData("Consultar")]
    [InlineData("Otro")]
    public void All_Valid_Operations_Are_Recognized(string operation)
    {
        AuditOperationType.IsValid(operation).Should().BeTrue();
    }

    [Fact]
    public void Invalid_Operation_Returns_False()
    {
        AuditOperationType.IsValid("NoExiste").Should().BeFalse();
    }

    [Fact]
    public void Null_Operation_Returns_False()
    {
        AuditOperationType.IsValid(null).Should().BeFalse();
    }
}
