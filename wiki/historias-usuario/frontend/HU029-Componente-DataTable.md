# HU029 — Componente DataTable Reutilizable

**Módulo:** Componentes Compartidos  
**Rol:** N/A (componente técnico)  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** desarrollador  
**Quiero** un componente de grilla reutilizable con búsqueda, filtros, paginación y exportación  
**Para** mantener consistencia visual y funcional en todas las listas del sistema

## Criterios de Aceptación

1. **DADO** que se usa DataTable en una página, **CUANDO** se configura con columnas, **ENTONCES** renderiza una tabla con headers y filas

2. **DADO** que tiene campo de búsqueda, **CUANDO** el usuario escribe, **ENTONCES** filtra las filas por las columnas configuradas como searchable

3. **DADO** que tiene filtros avanzados, **CUANDO** se aplican, **ENTONCES** filtra por valores de columna incluyendo rangos de fechas

4. **DADO** que los datos exceden una página, **CUANDO** se muestra, **ENTONCES** pagina los resultados con controles de navegación (configurable: items por página)

5. **DADO** que hay datos, **CUANDO** presiona exportar, **ENTONCES** puede descargar en:
   - **Excel (.xlsx)** usando la librería xlsx
   - **CSV** usando la librería xlsx

6. **DADO** que cada fila tiene acciones, **CUANDO** hace hover, **ENTONCES** muestra botones de acción (editar, eliminar) configurables

7. **DADO** que una fila es clickeable, **CUANDO** hace click, **ENTONCES** ejecuta la función de navegación configurada (onRowClick)

## Componentes Involucrados

- `src/components/shared/DataTable.tsx`
