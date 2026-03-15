# HU007 — Ver Detalle de Entidad

**Módulo:** Organización  
**Rol:** Todos los roles con "Ver Entidades"  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Entidades"  
**Quiero** ver el detalle completo de una entidad financiera  
**Para** consultar su información, sucursales y usuarios asociados

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/entidades/:id`, **CUANDO** carga, **ENTONCES** se muestra la información: nombre, CUIT, tipo, estado, email, teléfono, dirección, provincia, ciudad, canales, organización padre

2. **DADO** que la entidad tiene sucursales, **CUANDO** se visualiza, **ENTONCES** se listan las sucursales con código, dirección y estado

3. **DADO** que la entidad tiene usuarios asignados, **CUANDO** se visualiza, **ENTONCES** se listan los usuarios con rol y estado

4. **DADO** que el usuario presiona "Editar", **CUANDO** navega, **ENTONCES** se redirige a `/entidades/:id/editar`

## Componentes Involucrados

- `src/pages/entities/EntityDetail.tsx`
