# HU004 — Ver Detalle de Organización

**Módulo:** Organización  
**Rol:** Super Admin, Admin Entidad, Auditor, Consulta  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Organizaciones"  
**Quiero** ver el detalle completo de una organización  
**Para** consultar toda la información del grupo empresario y sus entidades asociadas

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/tenants/:id`, **CUANDO** la página carga, **ENTONCES** se muestra la información completa de la organización incluyendo nombre, CUIT, estado, contacto y país

2. **DADO** que la organización tiene entidades asociadas, **CUANDO** se visualiza el detalle, **ENTONCES** se muestra una lista de entidades dependientes

3. **DADO** que el usuario está en el detalle, **CUANDO** presiona "Editar", **ENTONCES** se navega a `/tenants/:id/editar`

4. **DADO** que la organización no existe, **CUANDO** se intenta acceder, **ENTONCES** se muestra un mensaje "No encontrado"

## Componentes Involucrados

- `src/pages/tenants/TenantDetail.tsx`
