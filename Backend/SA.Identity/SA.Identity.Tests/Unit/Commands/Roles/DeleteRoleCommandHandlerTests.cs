using FluentAssertions;
using NSubstitute;
using SA.Identity.Application.Commands.Roles;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Roles;

public sealed class DeleteRoleCommandHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly DeleteRoleCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid DeletedBy = Guid.NewGuid();

    public DeleteRoleCommandHandlerTests()
    {
        _sut = new DeleteRoleCommandHandler(_roleRepository, _eventPublisher);
    }

    private static void SetPrivateProperty<T>(T obj, string propertyName, object value) where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        prop!.SetValue(obj, value);
    }

    private DeleteRoleCommand BuildCommand(Guid? roleId = null)
    {
        return new DeleteRoleCommand(roleId ?? Guid.NewGuid(), TenantId, DeletedBy);
    }

    private static Role CreateRole(string name = "CustomRole", bool isSystem = false, Guid? id = null)
    {
        var role = Role.Create(TenantId, name, "desc", Guid.NewGuid());
        if (id.HasValue)
            SetPrivateProperty(role, nameof(Role.Id), id.Value);
        if (isSystem)
            SetPrivateProperty(role, nameof(Role.IsSystem), true);
        return role;
    }

    /// <summary>
    /// TP-SEC-12-01: DELETE /roles/:id elimina rol sin usuarios y retorna 204
    /// </summary>
    [Fact]
    public async Task Handle_ValidRoleNoUsers_DeletesRoleSuccessfully()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(id: roleId);
        var command = BuildCommand(roleId);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.HasAssignedUsersAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _roleRepository.Received(1).Delete(role);
        await _roleRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-12-02: role_permissions eliminadas junto con el rol
    /// </summary>
    [Fact]
    public async Task Handle_ValidRole_ClearsPermissionsBeforeDelete()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(id: roleId);
        var command = BuildCommand(roleId);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.HasAssignedUsersAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert - Permissions cleared before delete
        await _roleRepository.Received(1).SetRolePermissionsAsync(
            roleId,
            Arg.Is<IEnumerable<Guid>>(ids => !ids.Any()),
            Arg.Any<CancellationToken>());
        _roleRepository.Received(1).Delete(role);
    }

    /// <summary>
    /// TP-SEC-12-03: DomainEvent (role_deleted) publicado
    /// </summary>
    [Fact]
    public async Task Handle_ValidDelete_PublishesDomainEvent()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "ToDelete", id: roleId);
        var command = BuildCommand(roleId);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.HasAssignedUsersAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<Shared.Contract.Events.DomainEvent>(e =>
                e.Action == "role_deleted" &&
                e.EntityId == roleId &&
                e.Operation == "DELETE"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-12-04: Rol de sistema retorna 403
    /// </summary>
    [Fact]
    public async Task Handle_SystemRole_ThrowsForbidden()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "Super Admin", isSystem: true, id: roleId);
        var command = BuildCommand(roleId);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se puede eliminar un rol de sistema*");
    }

    /// <summary>
    /// TP-SEC-12-05: Rol con usuarios asignados retorna 409
    /// </summary>
    [Fact]
    public async Task Handle_RoleWithUsers_ThrowsConflict()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(id: roleId);
        var command = BuildCommand(roleId);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.HasAssignedUsersAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*tiene usuarios asignados*");
    }

    /// <summary>
    /// TP-SEC-12-06: Rol inexistente retorna 404
    /// </summary>
    [Fact]
    public async Task Handle_RoleNotFound_ThrowsNotFound()
    {
        // Arrange
        var command = BuildCommand();
        _roleRepository.GetByIdWithPermissionsAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Rol no encontrado*");
    }

    /// <summary>
    /// TP-SEC-12-01b: Delete does not call HasAssignedUsersAsync for system roles (fails earlier)
    /// </summary>
    [Fact]
    public async Task Handle_SystemRole_DoesNotCheckUsers()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(isSystem: true, id: roleId);
        var command = BuildCommand(roleId);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        // Act
        try { await _sut.Handle(command, CancellationToken.None); } catch { /* expected */ }

        // Assert
        await _roleRepository.DidNotReceive().HasAssignedUsersAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
