using System.Security.Cryptography;
using System.Text;
using SA.Identity.Domain.Entities;

namespace SA.Identity.DataAccess.EntityFramework.Seed;

/// <summary>
/// Seed data for the 88 atomic permissions organized in 15 modules.
/// Permissions are global (no tenant_id) and only modified via migrations.
/// See RN-SEC-17 in RF-SEC.md.
/// </summary>
public static class PermissionSeedData
{
    /// <summary>
    /// Generates a deterministic GUID from the permission code string.
    /// Ensures consistent IDs across environments and migrations.
    /// </summary>
    private static Guid DeterministicGuid(string code)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(code));
        return new Guid(hash);
    }

    /// <summary>
    /// Internal record used to hold permission definition tuples.
    /// </summary>
    public sealed record PermissionDefinition(string Module, string Action, string Code, string Description);

    private static readonly PermissionDefinition[] Definitions =
    [
        // ── 1. Organizaciones (6) ──────────────────────────────────────
        new("organizaciones", "list",   "p_org_list",    "Listar organizaciones"),
        new("organizaciones", "create", "p_org_create",  "Crear organizacion"),
        new("organizaciones", "edit",   "p_org_edit",    "Editar organizacion"),
        new("organizaciones", "delete", "p_org_delete",  "Eliminar organizacion"),
        new("organizaciones", "detail", "p_org_detail",  "Ver detalle de organizacion"),
        new("organizaciones", "export", "p_org_export",  "Exportar organizaciones"),

        // ── 2. Entidades (6) ───────────────────────────────────────────
        new("entidades", "list",   "p_ent_list",    "Listar entidades"),
        new("entidades", "create", "p_ent_create",  "Crear entidad"),
        new("entidades", "edit",   "p_ent_edit",    "Editar entidad"),
        new("entidades", "delete", "p_ent_delete",  "Eliminar entidad"),
        new("entidades", "detail", "p_ent_detail",  "Ver detalle de entidad"),
        new("entidades", "export", "p_ent_export",  "Exportar entidades"),

        // ── 3. Sucursales (4) ──────────────────────────────────────────
        new("sucursales", "list",   "p_branch_list",   "Listar sucursales"),
        new("sucursales", "create", "p_branch_create", "Crear sucursal"),
        new("sucursales", "edit",   "p_branch_edit",   "Editar sucursal"),
        new("sucursales", "delete", "p_branch_delete", "Eliminar sucursal"),

        // ── 4. Usuarios (4) ───────────────────────────────────────────
        new("usuarios", "list",   "p_users_list",   "Listar usuarios"),
        new("usuarios", "create", "p_users_create", "Crear usuario"),
        new("usuarios", "edit",   "p_users_edit",   "Editar usuario"),
        new("usuarios", "detail", "p_users_detail", "Ver detalle de usuario"),

        // ── 5. Roles (4) ──────────────────────────────────────────────
        new("roles", "list",   "p_roles_list",   "Listar roles"),
        new("roles", "create", "p_roles_create", "Crear rol"),
        new("roles", "edit",   "p_roles_edit",   "Editar rol"),
        new("roles", "delete", "p_roles_delete", "Eliminar rol"),

        // ── 6. Catalogo Familias (4) ──────────────────────────────────
        new("catalogo_familias", "list",   "p_cat_family_list",   "Listar familias de productos"),
        new("catalogo_familias", "create", "p_cat_family_create", "Crear familia de productos"),
        new("catalogo_familias", "edit",   "p_cat_family_edit",   "Editar familia de productos"),
        new("catalogo_familias", "delete", "p_cat_family_delete", "Eliminar familia de productos"),

        // ── 7. Catalogo Productos (7) ─────────────────────────────────
        new("catalogo_productos", "list",      "p_cat_list",      "Listar productos"),
        new("catalogo_productos", "create",    "p_cat_create",    "Crear producto"),
        new("catalogo_productos", "edit",      "p_cat_edit",      "Editar producto"),
        new("catalogo_productos", "delete",    "p_cat_delete",    "Eliminar producto"),
        new("catalogo_productos", "detail",    "p_cat_detail",    "Ver detalle de producto"),
        new("catalogo_productos", "export",    "p_cat_export",    "Exportar productos"),
        new("catalogo_productos", "deprecate", "p_cat_deprecate", "Deprecar producto"),

        // ── 8. Catalogo Planes (3) ────────────────────────────────────
        new("catalogo_planes", "create", "p_cat_plan_create", "Crear plan de producto"),
        new("catalogo_planes", "edit",   "p_cat_plan_edit",   "Editar plan de producto"),
        new("catalogo_planes", "delete", "p_cat_plan_delete", "Eliminar plan de producto"),

        // ── 9. Catalogo Coberturas (3) ────────────────────────────────
        new("catalogo_coberturas", "create", "p_cat_coverage_create", "Crear cobertura"),
        new("catalogo_coberturas", "edit",   "p_cat_coverage_edit",   "Editar cobertura"),
        new("catalogo_coberturas", "delete", "p_cat_coverage_delete", "Eliminar cobertura"),

        // ── 10. Catalogo Requisitos (3) ───────────────────────────────
        new("catalogo_requisitos", "create", "p_cat_req_create", "Crear requisito"),
        new("catalogo_requisitos", "edit",   "p_cat_req_edit",   "Editar requisito"),
        new("catalogo_requisitos", "delete", "p_cat_req_delete", "Eliminar requisito"),

        // ── 11. Catalogo Comisiones (4) ───────────────────────────────
        new("catalogo_comisiones", "list",   "p_com_list",   "Listar planes de comision"),
        new("catalogo_comisiones", "create", "p_com_create", "Crear plan de comision"),
        new("catalogo_comisiones", "edit",   "p_com_edit",   "Editar plan de comision"),
        new("catalogo_comisiones", "delete", "p_com_delete", "Eliminar plan de comision"),

        // ── 12. Solicitudes (6) ───────────────────────────────────────
        new("solicitudes", "list",       "p_app_list",       "Listar solicitudes"),
        new("solicitudes", "create",     "p_app_create",     "Crear solicitud"),
        new("solicitudes", "edit",       "p_app_edit",       "Editar solicitud"),
        new("solicitudes", "detail",     "p_app_detail",     "Ver detalle de solicitud"),
        new("solicitudes", "transition", "p_app_transition", "Transicionar solicitud"),
        new("solicitudes", "export",     "p_app_export",     "Exportar solicitudes"),

        // ── 13. Liquidaciones (4) ─────────────────────────────────────
        new("liquidaciones", "list",   "p_settle_list",   "Listar liquidaciones"),
        new("liquidaciones", "create", "p_settle_create", "Crear liquidacion"),
        new("liquidaciones", "detail", "p_settle_detail", "Ver detalle de liquidacion"),
        new("liquidaciones", "export", "p_settle_export", "Exportar liquidaciones"),

        // ── 14. Configuracion (18) ────────────────────────────────────
        // Parametros (4)
        new("configuracion", "param_list",   "p_cfg_param_list",   "Listar parametros de configuracion"),
        new("configuracion", "param_create", "p_cfg_param_create", "Crear parametro de configuracion"),
        new("configuracion", "param_edit",   "p_cfg_param_edit",   "Editar parametro de configuracion"),
        new("configuracion", "param_delete", "p_cfg_param_delete", "Eliminar parametro de configuracion"),
        // Servicios externos (5)
        new("configuracion", "svc_list",   "p_cfg_svc_list",   "Listar servicios externos"),
        new("configuracion", "svc_create", "p_cfg_svc_create", "Crear servicio externo"),
        new("configuracion", "svc_edit",   "p_cfg_svc_edit",   "Editar servicio externo"),
        new("configuracion", "svc_delete", "p_cfg_svc_delete", "Eliminar servicio externo"),
        new("configuracion", "svc_test",   "p_cfg_svc_test",   "Probar conexion de servicio externo"),
        // Workflows (5)
        new("configuracion", "wf_list",       "p_cfg_wf_list",       "Listar workflows"),
        new("configuracion", "wf_create",     "p_cfg_wf_create",     "Crear workflow"),
        new("configuracion", "wf_edit",       "p_cfg_wf_edit",       "Editar workflow"),
        new("configuracion", "wf_activate",   "p_cfg_wf_activate",   "Activar workflow"),
        new("configuracion", "wf_deactivate", "p_cfg_wf_deactivate", "Desactivar workflow"),
        // Plantillas de notificacion (4)
        new("configuracion", "tpl_list",   "p_cfg_tpl_list",   "Listar plantillas de notificacion"),
        new("configuracion", "tpl_create", "p_cfg_tpl_create", "Crear plantilla de notificacion"),
        new("configuracion", "tpl_edit",   "p_cfg_tpl_edit",   "Editar plantilla de notificacion"),
        new("configuracion", "tpl_delete", "p_cfg_tpl_delete", "Eliminar plantilla de notificacion"),

        // ── 15. Auditoria (2) ─────────────────────────────────────────
        new("auditoria", "list",   "p_audit_list",   "Consultar log de auditoria"),
        new("auditoria", "export", "p_audit_export", "Exportar log de auditoria"),
    ];

    /// <summary>
    /// Returns seed objects for EF Core HasData.
    /// Uses anonymous-style object array compatible with EF Core model seeding.
    /// Permission entity has private setters, so we create instances via reflection.
    /// </summary>
    public static object[] GetSeedObjects()
    {
        return Definitions
            .Select(d => (object)new
            {
                Id = DeterministicGuid(d.Code),
                d.Module,
                d.Action,
                d.Code,
                Description = (string?)d.Description
            })
            .ToArray();
    }

    /// <summary>
    /// Returns all 88 permission codes.
    /// </summary>
    public static IReadOnlyList<string> GetAllPermissionCodes() =>
        Definitions.Select(d => d.Code).ToList();

    /// <summary>
    /// Returns all permission definitions for use by other seed classes or services.
    /// </summary>
    public static IReadOnlyList<PermissionDefinition> GetDefinitions() => Definitions;

    /// <summary>
    /// Returns the deterministic GUID for a permission code.
    /// </summary>
    public static Guid GetPermissionId(string code) => DeterministicGuid(code);

    /// <summary>
    /// Total count of permissions. Should always be 88.
    /// </summary>
    public static int Count => Definitions.Length;
}
