# HU028 — Navegación del Sistema (Sidebar)

**Módulo:** General / Layout  
**Rol:** Todos los usuarios autenticados  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario del sistema  
**Quiero** navegar entre los diferentes módulos usando un menú lateral  
**Para** acceder rápidamente a las funcionalidades del backoffice

## Criterios de Aceptación

1. **DADO** que el usuario está autenticado, **CUANDO** accede a cualquier página, **ENTONCES** se muestra un sidebar izquierdo con las secciones:

   | Sección | Ítems | Íconos |
   |---------|-------|--------|
   | General | Dashboard | LayoutDashboard |
   | Organización | Organizaciones, Entidades | Layers, Building2 |
   | Operaciones | Solicitudes | FileCheck |
   | Catálogo | Familia, Productos | FolderTree, Package |
   | Configuración | Parámetros, Servicios, Planes de comisiones, Workflows | Settings, Unplug, BadgePercent, GitBranch |
   | Seguridad | Usuarios, Roles y Permisos, Auditoría | Users, Shield, ClipboardList |

2. **DADO** que el usuario navega a una ruta, **CUANDO** la ruta coincide con un ítem del sidebar, **ENTONCES** el ítem se resalta como activo

3. **DADO** que el sidebar se muestra, **CUANDO** el usuario está en un dispositivo móvil, **ENTONCES** el sidebar se comporta como un drawer colapsable

4. **DADO** que se muestra el sidebar, **CUANDO** se renderiza, **ENTONCES** incluye el logotipo o nombre de la aplicación en la parte superior

## Componentes Involucrados

- `src/components/layout/AppSidebar.tsx`
- `src/components/layout/AppLayout.tsx`
- `src/components/NavLink.tsx`
