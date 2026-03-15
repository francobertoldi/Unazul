# HU022 — Editor Visual de Workflows

**Módulo:** Configuración
**Rol:** Super Admin, Diseñador de Procesos
**Prioridad:** Alta
**Estado:** Implementada

---

## Descripción

**Como** diseñador de procesos
**Quiero** crear y editar workflows de forma visual usando un editor drag-and-drop
**Para** definir los flujos de proceso que gobiernan el ciclo de vida de las solicitudes

## Criterios de Aceptación

### Lista de Workflows

1. **DADO** que el usuario accede a `/workflows`, **CUANDO** carga, **ENTONCES** se muestra una grilla con:
   - Nombre
   - Descripción
   - Versión
   - Estado (draft, active, inactive)
   - Fecha de creación

### Editor Visual

2. **DADO** que el usuario crea (`/workflows/nuevo`) o edita (`/workflows/:id/editar`) un workflow, **CUANDO** carga, **ENTONCES** se muestra:
   - Panel superior: nombre, descripción, versión, estado
   - Canvas de React Flow (@xyflow/react) con nodos arrastrables

3. **DADO** que el editor está activo, **CUANDO** el usuario agrega un nodo, **ENTONCES** puede crear estados de los siguientes tipos (alineados con `flow_node_type`):
   - `start` — Estado inicial (solo uno por workflow)
   - `end` — Estado final (puede haber más de uno: aprobado, rechazado)
   - `service_call` — Invocación a servicio externo
   - `decision` — Nodo de decisión condicional (ver HU034 para panel de atributos)
   - `send_message` — Envío de notificación (email/sms/whatsapp)
   - `data_capture` — Captura de datos del usuario (formulario dinámico)
   - `timer` — Espera temporizada

### Configuración por tipo de nodo

4. **DADO** que el usuario hace click en un nodo, **CUANDO** se abre el diálogo de configuración (NodeConfigDialog), **ENTONCES** muestra campos comunes + campos específicos según el tipo:
   - **Comunes a todos:** Nombre del estado, SLA en horas (opcional)
   - **service_call:** Servicio externo (select desde `external_services`), endpoint, método HTTP
   - **decision:** Condición (expresión evaluable) — ver HU034
   - **send_message:** Canal (email/sms/whatsapp), plantilla de notificación (select desde `notification_templates`)
   - **data_capture:** Campos del formulario dinámico (CRUD inline: nombre, tipo, requerido, orden) — persiste en `workflow_state_fields`
   - **timer:** Minutos de espera

   Todos los valores de configuración específicos se persisten en `workflow_state_configs` como pares key-value.

### Transiciones

5. **DADO** que existen nodos, **CUANDO** el usuario conecta dos nodos arrastrando, **ENTONCES** se crea una transición con:
   - Label (nombre de la acción)
   - Condición (opcional, para bifurcaciones)
   - SLA en horas (opcional)

### Validación y guardado

6. **DADO** que el workflow se guarda, **CUANDO** se valida, **ENTONCES** debe cumplir:
   - Exactamente un nodo `start`
   - Al menos un nodo `end`
   - Todos los nodos conectados (sin nodos huérfanos)
   - Sin ciclos que no pasen por un nodo `end`

## Componentes Involucrados

- `src/pages/workflows/WorkflowList.tsx`
- `src/pages/workflows/WorkflowEditor.tsx`
- `src/components/workflow/FlowNode.tsx`
- `src/components/workflow/NodeConfigDialog.tsx`
