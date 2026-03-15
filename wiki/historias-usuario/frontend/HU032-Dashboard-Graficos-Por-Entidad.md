# HU032 — Dashboard: Gráficos por Entidad

**Módulo:** General  
**Rol:** Todos los usuarios autenticados  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** usuario del sistema  
**Quiero** ver gráficos individuales por cada entidad mostrando sus productos  
**Para** analizar la distribución de productos y su valor económico por entidad

## Criterios de Aceptación

1. **DADO** que el usuario accede al Dashboard, **CUANDO** la página carga, **ENTONCES** se muestra un gráfico de barras individual por cada entidad que tenga productos.

2. **DADO** que se renderiza un gráfico de entidad, **CUANDO** el usuario observa las barras, **ENTONCES** se visualizan dos ejes:
   - Eje izquierdo: Cantidad de productos
   - Eje derecho: Valor representativo acumulado (formateado como moneda)

3. **DADO** que el valor representativo depende del tipo de producto, **CUANDO** se calcula, **ENTONCES** se normaliza según la familia:
   - Seguros → Suma asegurada total de coberturas
   - Tarjetas → Límite de crédito
   - Cuentas → Cuota de mantenimiento
   - Inversiones → Monto mínimo
   - Préstamos → Precio del plan

4. **DADO** que hay múltiples entidades, **CUANDO** se renderizan los gráficos, **ENTONCES** se presentan en un grid de 2 columnas en pantallas grandes.

## Componentes Involucrados

- `src/pages/Dashboard.tsx`

## Datos de Entrada

Datos de los stores `useProductStore` y `useEntityStore`.

## Datos de Salida

Grid de gráficos de barras con doble eje Y por entidad.
