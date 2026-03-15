# HU036 — Historial de Liquidaciones

**Módulo:** Operaciones
**Rol:** Super Admin, Admin Entidad, Operador, Auditor, Consulta
**Prioridad:** Alta
**Estado:** Pendiente

---

## Descripción

**Como** usuario con permisos de "Ver Liquidaciones"
**Quiero** consultar el historial de liquidaciones realizadas
**Para** revisar las liquidaciones pasadas, sus montos y descargar los reportes

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/liquidaciones`, **CUANDO** carga, **ENTONCES** se muestra una grilla con:
   - Fecha de liquidación (formato es-AR)
   - Ejecutado por (nombre del usuario)
   - Cantidad de operaciones
   - Totales por moneda (columnas dinámicas o badges)
   - Reporte Excel (ícono de descarga si existe)

2. **DADO** que hay liquidaciones, **CUANDO** busca, **ENTONCES** filtra por rango de fechas y usuario

3. **DADO** que hace click en una fila, **CUANDO** navega al detalle, **ENTONCES** se muestra:
   - Resumen: fecha, usuario, cantidad de operaciones, totales por moneda
   - Grilla de items: código solicitud, solicitante, producto, plan, tipo comisión, valor, monto calculado, moneda, fórmula

4. **DADO** que la liquidación tiene reporte Excel, **CUANDO** presiona descargar, **ENTONCES** se descarga el archivo desde `{STORAGE_ROOT}/{excel_url}`

5. **DADO** que exporta la vista, **CUANDO** descarga, **ENTONCES** obtiene archivo Excel o CSV

## Componentes Involucrados

- `src/pages/settlements/SettlementList.tsx`
- `src/pages/settlements/SettlementDetail.tsx`
- `src/components/shared/DataTable.tsx`
