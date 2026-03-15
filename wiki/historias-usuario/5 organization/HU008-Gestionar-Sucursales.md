# HU008 — Gestionar Sucursales

**Módulo:** Organización  
**Rol:** Super Admin, Admin Entidad  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** administrador  
**Quiero** crear, editar y eliminar sucursales de una entidad  
**Para** gestionar las unidades operativas de cada organización financiera

## Criterios de Aceptación

1. **DADO** que el usuario edita una entidad, **CUANDO** observa la sección de sucursales, **ENTONCES** ve una grilla con: nombre, código, dirección, ciudad, provincia, estado, responsable, teléfono

2. **DADO** que presiona "Agregar Sucursal", **CUANDO** navega a `/entidades/:entityId/sucursales/nuevo`, **ENTONCES** se muestra un formulario con campos:
   - Nombre (requerido)
   - Código (requerido)
   - Dirección (requerido)
   - Provincia (select parametrizado, requerido)
   - Ciudad (select en cascada, requerido)
   - Estado (requerido)
   - Responsable (requerido)
   - Código de teléfono (requerido)
   - Teléfono (requerido)

3. **DADO** que el usuario selecciona una provincia en el formulario de sucursal, **CUANDO** cambia el valor, **ENTONCES** las ciudades se filtran automáticamente

4. **DADO** que presiona editar en una sucursal, **CUANDO** navega, **ENTONCES** el formulario se precarga con los datos existentes

5. **DADO** que presiona eliminar, **CUANDO** confirma, **ENTONCES** la sucursal se elimina de la entidad

## Componentes Involucrados

- `src/pages/entities/BranchForm.tsx`
- `src/pages/entities/EntityForm.tsx` (grilla de sucursales)
