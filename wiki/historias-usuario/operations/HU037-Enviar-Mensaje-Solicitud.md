# HU037 — Enviar Mensaje desde Solicitud

**Módulo:** Operaciones
**Rol:** Super Admin, Admin Entidad, Operador
**Prioridad:** Media
**Estado:** Pendiente

---

## Descripción

**Como** operador
**Quiero** enviar mensajes (email, SMS o WhatsApp) al solicitante desde el detalle de una solicitud
**Para** comunicar novedades, solicitar documentación o notificar resoluciones

## Criterios de Aceptación

1. **DADO** que el usuario está en el detalle de una solicitud, **CUANDO** presiona "Enviar Mensaje", **ENTONCES** se abre un diálogo con:
   - Canal (select: email, sms, whatsapp)
   - Plantilla (select desde `notification_templates`, filtrado por canal seleccionado)
   - Destinatario (precargado desde los contactos del solicitante, editable)
   - Asunto (solo para email, precargado desde plantilla)
   - Cuerpo del mensaje (precargado desde plantilla con variables resueltas: {{nombre}}, {{codigo_solicitud}}, etc.)

2. **DADO** que el usuario selecciona una plantilla, **CUANDO** cambia, **ENTONCES** el cuerpo se actualiza con el contenido de la plantilla y las variables disponibles se resuelven con datos de la solicitud y el solicitante

3. **DADO** que el usuario presiona "Enviar", **CUANDO** se confirma, **ENTONCES**:
   - Se publica `MessageSentEvent` via RabbitMQ
   - Notification Service consume el evento y despacha al proveedor configurado (SMTP, Twilio, Meta)
   - Se registra en `notification_log` con estado (sent/failed/pending)

4. **DADO** que el mensaje fue enviado, **CUANDO** se visualiza en observaciones, **ENTONCES** se agrega una observación automática con el detalle del envío (canal, destinatario, plantilla usada)

5. **DADO** que el proveedor externo falla, **CUANDO** se registra el error, **ENTONCES** `notification_log.status` queda como `failed` con `error_message` y se muestra una alerta al operador

## Componentes Involucrados

- `src/pages/applications/ApplicationDetail.tsx` (botón Enviar Mensaje)
- `src/components/applications/SendMessageDialog.tsx`
