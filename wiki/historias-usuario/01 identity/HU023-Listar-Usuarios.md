# HU023 — Listar Usuarios

**Módulo:** Seguridad  
**Rol:** Super Admin, Admin Entidad, Auditor, Consulta  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** administrador  
**Quiero** ver un listado de todos los usuarios del sistema  
**Para** gestionar las cuentas de acceso

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/usuarios`, **CUANDO** carga, **ENTONCES** se muestra una grilla con:
   - Nombre de usuario
   - Email
   - Nombre completo
   - Entidad asignada
   - Roles
   - Estado (active, inactive, locked — con badge semántico)
   - Último login
   - Fecha de creación

2. **DADO** que busca, **CUANDO** escribe, **ENTONCES** filtra por nombre de usuario y email

3. **DADO** que hace click en fila, **CUANDO** navega, **ENTONCES** redirige a `/usuarios/:id`

4. **DADO** que exporta, **CUANDO** descarga, **ENTONCES** obtiene Excel o CSV

## Componentes Involucrados

- `src/pages/security/UserList.tsx`
- `src/components/shared/DataTable.tsx`
