# HU001 — Visualizar Dashboard

**Módulo:** General  
**Rol:** Todos los usuarios autenticados  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario del sistema  
**Quiero** ver un panel principal con métricas resumidas  
**Para** tener una visión general del estado de la operación

## Criterios de Aceptación

1. **DADO** que el usuario accede a la ruta `/`, **CUANDO** la página carga, **ENTONCES** se muestran 4 tarjetas de métricas:
   - Total de entidades activas
   - Solicitudes en proceso
   - Usuarios activos
   - Productos publicados

2. **DADO** que las métricas se muestran, **CUANDO** el usuario observa las tarjetas, **ENTONCES** cada una tiene:
   - Ícono representativo
   - Título descriptivo
   - Valor numérico
   - Indicador de tendencia (porcentaje de cambio)

3. **DADO** que el dashboard se carga, **CUANDO** se renderizan los gráficos, **ENTONCES** se muestran gráficos de tendencia utilizando Recharts

## Componentes Involucrados

- `src/pages/Dashboard.tsx`
- `src/components/shared/MetricCard.tsx`

## Datos de Entrada

N/A — Los datos se obtienen de los stores del sistema.

## Datos de Salida

Panel visual con métricas y gráficos.
