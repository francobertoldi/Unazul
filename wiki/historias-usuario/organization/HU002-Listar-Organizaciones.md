# HU002 — Listar Organizaciones

**Módulo:** Organización  
**Rol:** Super Admin, Admin Entidad, Auditor, Consulta  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Organizaciones"  
**Quiero** ver un listado de todas las organizaciones (tenants) del sistema  
**Para** gestionar y consultar los grupos empresarios registrados

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/tenants`, **CUANDO** la página carga, **ENTONCES** se muestra una grilla (DataTable) con las columnas:
   - Nombre
   - Identificador (CUIT)
   - Estado (badge con color semántico)
   - País
   - Contacto
   - Fecha de Creación

2. **DADO** que hay organizaciones registradas, **CUANDO** el usuario escribe en el campo de búsqueda, **ENTONCES** se filtran los resultados por nombre e identificador

3. **DADO** que el usuario observa una fila, **CUANDO** hace hover, **ENTONCES** se muestran acciones: ver detalle, editar, eliminar

4. **DADO** que el usuario hace click en una fila, **CUANDO** navega, **ENTONCES** se redirige al detalle de la organización (`/tenants/:id`)

5. **DADO** que el usuario presiona "Eliminar", **CUANDO** confirma en el diálogo, **ENTONCES** la organización se elimina y se muestra una notificación de éxito

6. **DADO** que hay datos, **CUANDO** el usuario usa los botones de exportación, **ENTONCES** puede descargar la grilla en formato Excel (.xlsx) o CSV

## Componentes Involucrados

- `src/pages/tenants/TenantList.tsx`
- `src/components/shared/DataTable.tsx`
- `src/components/crud/DeleteConfirmDialog.tsx`
