# HU025 — Ver Detalle de Usuario

**Módulo:** Seguridad  
**Rol:** Super Admin, Admin Entidad, Auditor, Consulta  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** administrador  
**Quiero** ver el detalle completo de un usuario  
**Para** consultar su información, roles y permisos efectivos

## Criterios de Aceptación

1. **DADO** que accede a `/usuarios/:id`, **CUANDO** carga, **ENTONCES** muestra: nombre de usuario, email, nombre completo, entidad, estado, último login, fecha creación

2. **DADO** que el usuario tiene roles, **CUANDO** se visualiza, **ENTONCES** se listan los roles asignados y los permisos efectivos resultantes

3. **DADO** que presiona "Editar", **ENTONCES** navega a `/usuarios/:id/editar`

## Componentes Involucrados

- `src/pages/security/UserDetail.tsx`
