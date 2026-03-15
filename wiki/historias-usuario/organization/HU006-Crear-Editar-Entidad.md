# HU006 — Crear y Editar Entidad

**Módulo:** Organización  
**Rol:** Super Admin  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** Super Admin  
**Quiero** crear y editar entidades financieras  
**Para** registrar nuevas organizaciones financieras o actualizar sus datos

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/entidades/nuevo`, **CUANDO** la página carga, **ENTONCES** se muestra un formulario con:
   - Organización (select con organizaciones existentes, requerido)
   - Nombre (requerido)
   - Identificador / CUIT (requerido)
   - Tipo de entidad (select parametrizado desde `entity_types`: bank, insurance, fintech, cooperative, sgr, regional_card; requerido)
   - Estado: active / inactive / suspended (requerido)
   - Email (requerido)
   - Teléfono (requerido)
   - Dirección (requerido)
   - Provincia (select parametrizado desde `provinces`, requerido)
   - Ciudad (select parametrizado desde `cities`, filtrado por provincia, requerido)
   - Canales habilitados (checkboxes parametrizados desde `channels`: web, mobile, api, presencial, ia_agent; requerido)

2. **DADO** que el usuario selecciona una provincia, **CUANDO** cambia el valor, **ENTONCES** el combo de ciudades se filtra automáticamente mostrando solo las ciudades de esa provincia (cascading select via `parentKey`)

3. **DADO** que se edita una entidad existente (`/entidades/:id/editar`), **CUANDO** carga, **ENTONCES** se muestran además:
   - Grilla de **Sucursales** con CRUD inline
   - Grilla de **Usuarios asignados** con navegación a edición

4. **DADO** que todos los campos requeridos están completos, **CUANDO** guarda, **ENTONCES** se crea/actualiza y redirige con notificación

## Campos del Formulario

| Campo | Tipo UI | Fuente de datos |
|-------|---------|----------------|
| tenantId | Select | Store de organizaciones |
| name | Input text | — |
| identifier | Input text | — |
| type | Select | Grupo `entity_types` (bank, insurance, fintech, cooperative, sgr, regional_card) |
| status | Select | Fijo: active, inactive, suspended |
| email | Input email | — |
| phone | Input text | — |
| address | Input text | — |
| province | Select | Grupo `provinces` |
| city | Select | Grupo `cities` (filtrado por provincia) |
| channels | Checkboxes | Grupo `channels` (web, mobile, api, presencial, ia_agent) |

## Componentes Involucrados

- `src/pages/entities/EntityForm.tsx`
