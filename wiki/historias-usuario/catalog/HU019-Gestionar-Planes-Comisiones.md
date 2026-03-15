# HU019 — Gestionar Planes de Comisiones

**Módulo:** Catálogo (backend: Catalog Service; UI: navegación bajo Configuración)
**Rol:** Super Admin  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** Super Admin  
**Quiero** crear, editar y eliminar planes de comisiones  
**Para** definir las comisiones que se asignarán a los sub productos

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/planes-comisiones`, **CUANDO** carga, **ENTONCES** se muestra una grilla con:
   - Código (máx 15 chars)
   - Descripción (máx 50 chars)
   - Tipo de comisión (Monto fijo por venta / Porcentaje del capital / Porcentaje del préstamo total)
   - Valor
   - Monto máximo (si aplica)

2. **DADO** que un plan de comisión ya está asignado a uno o más sub productos, **CUANDO** se visualiza en el selector, **ENTONCES** muestra un indicador de cuántos planes lo usan (relación 1:N permitida)

3. **DADO** que presiona "Nuevo Plan", **CUANDO** completa los campos, **ENTONCES** se crea el plan de comisiones

4. **DADO** que presiona eliminar, **CUANDO** confirma, **ENTONCES** se elimina si no está asignado a ningún sub producto

## Componentes Involucrados

- `src/pages/config/CommissionPlanList.tsx`
