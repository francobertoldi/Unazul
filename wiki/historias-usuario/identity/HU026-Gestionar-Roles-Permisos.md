# HU026 — Gestionar Roles y Permisos Atómicos

**Módulo:** Seguridad  
**Rol:** Super Admin  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** Super Admin  
**Quiero** crear, editar y eliminar roles con permisos atómicos granulares organizados por módulo  
**Para** definir con máxima precisión los niveles de acceso de cada rol del sistema

## Criterios de Aceptación

### Lista de Roles

1. **DADO** que accede a `/roles`, **CUANDO** carga, **ENTONCES** se muestra una grilla con:
   - Nombre del rol
   - Descripción
   - Cantidad de permisos asignados
   - Cantidad de usuarios con ese rol

### Crear/Editar Rol

2. **DADO** que accede a `/roles/nuevo` o `/roles/:id/editar`, **CUANDO** carga, **ENTONCES** muestra:
   - Nombre (requerido, min 2 chars, max 50)
   - Descripción (requerido, min 3 chars, max 200)
   - Secciones colapsables de permisos agrupados por módulo

3. **DADO** que se muestran los módulos de permisos, **CUANDO** se renderizan, **ENTONCES** cada módulo tiene:
   - **Checkbox maestro** que selecciona/deselecciona todos los permisos del módulo
   - Icono **ChevronDown** para colapsar/expandir
   - Nombre del módulo en negrita
   - Contador `(seleccionados/total)` en texto secundario
   - Secciones abiertas por defecto

4. **DADO** que se muestran los permisos individuales, **CUANDO** se renderizan, **ENTONCES** cada permiso tiene:
   - Checkbox individual
   - **Nombre de la acción** en negrita (ej. "Crear", "Editar", "Aprobar")
   - **Descripción** en texto muted después de un guión (ej. "— Crear nuevas solicitudes")

5. **DADO** que el usuario marca/desmarca el checkbox maestro de un módulo, **CUANDO** cambia, **ENTONCES** todos los permisos del módulo se seleccionan o deseleccionan

6. **DADO** que se muestra el encabezado de la card de permisos, **CUANDO** se renderiza, **ENTONCES** muestra el contador global `(seleccionados/total)` (ej. "Permisos (35/88)")

### Módulos de Permisos (15 módulos, 88 permisos)

7. **DADO** que se edita un rol, **CUANDO** se muestran los módulos, **ENTONCES** están disponibles:

| Módulo | Permisos | Acciones |
|--------|----------|----------|
| Organizaciones | 6 | Listar, Ver Detalle, Crear, Editar, Eliminar, Exportar |
| Entidades | 6 | Listar, Ver Detalle, Crear, Editar, Eliminar, Exportar |
| Sucursales | 4 | Listar, Crear, Editar, Eliminar |
| Solicitudes | 12 | Listar, Ver Detalle, Crear, Editar, Eliminar, Aprobar, Rechazar, Asignar, Agregar Observaciones, Gestionar Documentos, Ver Trazabilidad, Exportar |
| Familias de Productos | 4 | Listar, Crear, Editar, Eliminar |
| Productos | 6 | Listar, Ver Detalle, Crear, Editar, Eliminar, Exportar |
| Sub Productos | 3 | Crear, Editar, Eliminar |
| Requisitos | 3 | Crear, Editar, Eliminar |
| Planes de Comisiones | 4 | Listar, Crear, Editar, Eliminar |
| Workflows | 6 | Listar, Ver Detalle, Crear, Editar, Eliminar, Diseñar Flujos |
| Usuarios | 7 | Listar, Ver Detalle, Crear, Editar, Eliminar, Bloquear/Desbloquear, Exportar |
| Roles | 4 | Listar, Crear, Editar, Eliminar |
| Auditoría | 2 | Consultar, Exportar |
| Parámetros | 4 | Listar, Crear, Editar, Eliminar |
| Servicios Externos | 5 | Listar, Crear, Editar, Eliminar, Probar Conexión |

### Roles Predefinidos

8. **DADO** que el sistema se inicializa, **CUANDO** se cargan los datos, **ENTONCES** existen 7 roles predefinidos:

| Rol | Permisos | Alcance principal |
|-----|----------|------------------|
| Super Admin | 88 (todos) | Acceso total |
| Admin Entidad | ~35 | Gestión completa de entidad, solicitudes y usuarios |
| Operador | ~15 | Solicitudes y consulta de productos |
| Auditor | ~25 | Lectura global + auditoría |
| Consulta | ~18 | Solo lectura |
| Diseñador de Procesos | ~12 | Workflows y consulta de productos |
| Admin Producto | ~20 | Catálogo completo + comisiones |

### Validaciones

9. **DADO** que el usuario no selecciona ningún permiso, **CUANDO** intenta guardar, **ENTONCES** se muestra error "Seleccione al menos un permiso"

10. **DADO** que el formulario se envía con datos válidos, **CUANDO** se guarda, **ENTONCES** se muestra notificación de éxito y redirige a `/roles`

## Componentes Involucrados

- `src/pages/security/RolesPage.tsx`
- `src/pages/security/RoleForm.tsx`
- `src/data/mock.ts` (mockPermissions, mockRoles)
