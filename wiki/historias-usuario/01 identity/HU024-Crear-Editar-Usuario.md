# HU024 — Crear y Editar Usuario

**Módulo:** Seguridad  
**Rol:** Super Admin, Admin Entidad  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** administrador  
**Quiero** crear y editar usuarios del sistema  
**Para** gestionar las cuentas de acceso y sus permisos

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/usuarios/nuevo`, **CUANDO** carga, **ENTONCES** se muestra un formulario con:
   - Nombre de usuario (validado con máscara `mask.username`: `^[a-zA-Z0-9._-]{3,30}$`)
   - Contraseña (validada con máscara `mask.password`)
   - Email (requerido)
   - Nombre (requerido)
   - Apellido (requerido)
   - Entidad principal (select — incluye opción "_platform" para usuarios de plataforma). Mapea a `users.entity_id` (denormalizado)
   - Asignaciones (CRUD inline): permite asignar el usuario a múltiples alcances jerárquicos. Cada asignación tiene:
     - Tipo de alcance (select: organization, entity, branch)
     - Alcance (select dinámico según tipo: organizaciones, entidades o sucursales)
     - Persiste en tabla `user_assignments(scope_type, scope_id)`
   - Roles (selección múltiple de checkboxes, requerido al menos uno)
   - Estado: active, inactive, locked

2. **DADO** que el usuario selecciona roles, **CUANDO** se calculan los permisos, **ENTONCES** se muestra visualmente la unión de todos los permisos de los roles seleccionados (permisos efectivos)

3. **DADO** que las máscaras de validación están configuradas en parámetros, **CUANDO** se aplican, **ENTONCES** se cargan dinámicamente desde `mask.username` y `mask.password` del grupo Máscaras

4. **DADO** que se edita un usuario existente, **CUANDO** carga, **ENTONCES** el formulario se precarga con los datos y roles actuales

5. **DADO** que guarda, **CUANDO** las validaciones pasan, **ENTONCES** se crea/actualiza el usuario

## Componentes Involucrados

- `src/pages/security/UserForm.tsx`
