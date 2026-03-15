# HU012 — Trazabilidad de Solicitud (Timeline BPM)

**Módulo:** Operaciones  
**Rol:** Super Admin, Admin Entidad, Operador, Auditor, Consulta  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Solicitudes"  
**Quiero** ver la trazabilidad completa de una solicitud en formato de timeline visual  
**Para** conocer el progreso del trámite a través de los estados del workflow, identificar qué pasos se completaron, cuándo y por quién, y qué pasos restan

## Criterios de Aceptación

### Estructura Visual

1. **DADO** que el usuario accede a la solapa "Trazabilidad" del detalle de una solicitud, **CUANDO** carga, **ENTONCES** se muestra un timeline vertical estilo BPM con:
   - Columna izquierda: nodos circulares (40x40px) conectados por líneas verticales
   - Columna derecha: cards con información del estado

2. **DADO** que la solicitud tiene un workflow asociado, **CUANDO** se renderiza, **ENTONCES** se recorre el workflow desde el estado inicial siguiendo las transiciones, priorizando el camino principal (estados no finales primero)

### Estados Completados

3. **DADO** que un estado del workflow tiene un evento de trazabilidad (TraceEvent), **CUANDO** se renderiza, **ENTONCES**:
   - Nodo circular con fondo primario (bg-primary) y texto blanco
   - Ícono: CheckCircle2
   - Card con borde sólido y fondo bg-card
   - Contenido: nombre del estado, fecha/hora (formato dd/mm/yyyy HH:mm es-AR), nombre del usuario, acción realizada y detalle (si existe)

### Estado Actual

4. **DADO** que un estado es el estado actual de la solicitud y no es final, **CUANDO** se renderiza, **ENTONCES**:
   - Nodo circular con fondo primario + anillo animado (ring-4 ring-primary/20)
   - Ícono: Clock
   - Badge "En curso" con animación pulse
   - Si tiene SLA definido, se muestra "SLA: Xh"

### Estados Pendientes

5. **DADO** que un estado no tiene evento de trazabilidad y no es el estado actual, **CUANDO** se renderiza, **ENTONCES**:
   - Nodo circular con fondo muted y texto muted
   - Ícono: Circle (vacío)
   - Card con bordes punteados (border-dashed) y fondo muted/30
   - Texto: "Pendiente" en itálica

### Estados Finales

6. **DADO** que un estado final está completado y la solicitud está aprobada, **CUANDO** se renderiza, **ENTONCES**:
   - Nodo verde (bg-green-500) con texto blanco
   - Badge "Finalizado" con variante default

7. **DADO** que un estado final está completado y la solicitud está rechazada, **CUANDO** se renderiza, **ENTONCES**:
   - Nodo rojo (bg-destructive) con texto blanco
   - Badge "Rechazada" con variante destructive

### Líneas Conectoras

8. **DADO** que dos estados consecutivos están ambos completados, **CUANDO** se renderiza la línea entre ellos, **ENTONCES** la línea tiene color primario (bg-primary)

9. **DADO** que la transición va hacia un estado no completado, **CUANDO** se renderiza, **ENTONCES** la línea tiene color borde (bg-border)

### Sin Workflow

10. **DADO** que la solicitud no tiene un workflow asociado, **CUANDO** se accede a la solapa, **ENTONCES** se muestra "No se encontró un workflow asociado a esta solicitud."

## Datos del TraceEvent

| Campo | Tipo | Visualización |
|-------|------|--------------|
| workflowStateName | string | Título del nodo |
| timestamp | string (ISO) | Formato dd/mm/yyyy HH:mm (es-AR) |
| userName | string | Nombre del usuario que ejecutó la acción |
| action | string | Texto primario de la acción |
| detail | string (opcional) | Texto secundario muted |

## Componentes Involucrados

- `src/pages/applications/ApplicationDetail.tsx` (solapa Trazabilidad)
- `src/data/types.ts` (interfaces TraceEvent, WorkflowState, WorkflowTransition)
- `src/data/store.ts` (useApplicationStore, useWorkflowStore)
