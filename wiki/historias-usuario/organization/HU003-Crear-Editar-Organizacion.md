# HU003 — Crear y Editar Organización

**Módulo:** Organización  
**Rol:** Super Admin  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** Super Admin  
**Quiero** crear y editar organizaciones  
**Para** registrar nuevos grupos empresarios o actualizar sus datos

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/tenants/nuevo`, **CUANDO** la página carga, **ENTONCES** se muestra un formulario con los campos:
   - Nombre (requerido)
   - Identificador / CUIT (requerido)
   - Descripción (requerido)
   - Estado: active / inactive (requerido)
   - Nombre de contacto (requerido)
   - Email de contacto (requerido)
   - Código de teléfono de contacto (requerido)
   - Teléfono de contacto (requerido)
   - País (requerido)

2. **DADO** que el usuario completa todos los campos requeridos, **CUANDO** presiona "Guardar", **ENTONCES** la organización se crea y se redirige a la lista con notificación de éxito

3. **DADO** que el usuario accede a `/tenants/:id/editar`, **CUANDO** la página carga, **ENTONCES** el formulario se precarga con los datos existentes

4. **DADO** que se está editando una organización, **CUANDO** la página carga, **ENTONCES** se muestra además una grilla de entidades dependientes con navegación directa a la edición de cada entidad

5. **DADO** que un campo requerido está vacío, **CUANDO** el usuario intenta guardar, **ENTONCES** se muestran mensajes de validación inline

## Componentes Involucrados

- `src/pages/tenants/TenantForm.tsx`

## Campos del Formulario

| Campo | Tipo | Validación |
|-------|------|-----------|
| name | Input text | Requerido |
| identifier | Input text | Requerido |
| description | Textarea | Requerido |
| status | Select | Requerido |
| contactName | Input text | Requerido |
| contactEmail | Input email | Requerido, formato email |
| contactPhoneCode | Input text | Requerido |
| contactPhone | Input text | Requerido |
| country | Input text | Requerido |
