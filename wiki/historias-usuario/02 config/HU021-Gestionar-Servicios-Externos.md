# HU021 — Gestionar Servicios Externos

**Módulo:** Configuración  
**Rol:** Super Admin  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** Super Admin  
**Quiero** gestionar las conexiones a servicios y APIs externos  
**Para** configurar integraciones con sistemas de terceros

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/servicios`, **CUANDO** carga, **ENTONCES** se muestra una lista de servicios con:
   - Nombre
   - Descripción
   - Tipo (REST API, MCP, GraphQL, SOAP, Webhook)
   - URL base
   - Estado (active, inactive, error)
   - Tipo de autenticación (none, api_key, bearer_token, basic_auth, oauth2, custom_header)

2. **DADO** que presiona "Nuevo Servicio", **CUANDO** completa el formulario, **ENTONCES** se crea el servicio con:
   - Nombre, Descripción, Tipo, URL Base
   - Configuración de autenticación según el tipo seleccionado
   - Timeout (ms), Reintentos

3. **DADO** que un servicio existe, **CUANDO** presiona "Probar Conexión", **ENTONCES** se ejecuta una prueba y se registra el resultado (success/failure) con timestamp

4. **DADO** que edita un servicio, **CUANDO** cambia la configuración, **ENTONCES** se persisten los cambios

## Componentes Involucrados

- `src/pages/config/ServicesPage.tsx`
