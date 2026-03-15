# HU035 — Liquidar Comisiones

**Módulo:** Operaciones
**Rol:** Super Admin, Admin Entidad, Operador
**Prioridad:** Alta
**Estado:** Pendiente

---

## Descripción

**Como** operador
**Quiero** ejecutar la liquidación masiva de comisiones sobre solicitudes aprobadas
**Para** calcular y registrar las comisiones generadas por las operaciones del período

## Criterios de Aceptación

### Preliquidación

1. **DADO** que el usuario accede a `/liquidaciones/nueva`, **CUANDO** carga, **ENTONCES** se muestra un formulario de filtro con:
   - Entidad (select, opcional)
   - Rango de fechas (desde/hasta, requerido)
   - Estado de solicitud: solo `approved` (preseleccionado)

2. **DADO** que el usuario aplica los filtros, **CUANDO** presiona "Preliquidar", **ENTONCES** se muestra una grilla con las solicitudes candidatas y el cálculo estimado:
   - Código de solicitud
   - Solicitante
   - Producto / Plan
   - Tipo de comisión
   - Valor de comisión
   - Monto calculado
   - Moneda
   - Fórmula aplicada (legible)

3. **DADO** que se muestra la preliquidación, **CUANDO** se observa el pie de la grilla, **ENTONCES** se muestran los totales agrupados por moneda (mapea a `settlement_totals`)

### Confirmación

4. **DADO** que el usuario revisa la preliquidación, **CUANDO** presiona "Confirmar liquidación", **ENTONCES** se solicita confirmación con resumen: cantidad de operaciones, total por moneda

5. **DADO** que el usuario confirma, **CUANDO** se ejecuta la liquidación, **ENTONCES**:
   - Se crea un registro en `settlements` con `settled_at`, `settled_by`, `operation_count`
   - Se crean los `settlement_totals` por moneda
   - Se crean los `settlement_items` por cada solicitud liquidada
   - Las solicitudes pasan a estado `settled`
   - Se publica `CommissionsSettledEvent` via RabbitMQ (consume Audit y Notification)

### Exportación

6. **DADO** que la liquidación se completó, **CUANDO** se genera el reporte, **ENTONCES** se crea un archivo Excel en `{STORAGE_ROOT}/{tenant_id}/settlements/{yyyy}/{MM}/{settlement_id}.xlsx` y se almacena la ruta relativa en `settlements.excel_url`

7. **DADO** que la liquidación se completó, **CUANDO** el usuario presiona "Descargar Excel", **ENTONCES** se descarga el reporte generado

## Componentes Involucrados

- `src/pages/settlements/SettlementNew.tsx`
