# HU010 — Crear y Editar Solicitud

**Módulo:** Operaciones
**Rol:** Super Admin, Admin Entidad, Operador
**Prioridad:** Alta
**Estado:** Implementada

---

## Descripción

**Como** operador
**Quiero** crear y editar solicitudes de productos financieros
**Para** registrar nuevos trámites o actualizar la información de uno existente

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/solicitudes/nuevo`, **CUANDO** carga, **ENTONCES** se muestra un formulario con secciones:
   - **Datos Generales:** Entidad (select), Producto (select filtrado por entidad), Plan (select filtrado por producto)
   - **Solicitante:** Búsqueda por tipo y número de documento. Si existe, se precargan sus datos. Si no existe, se crea uno nuevo con: Nombre, Apellido, Tipo de Documento (DNI/CUIT/Pasaporte), Número de Documento, Fecha de Nacimiento, Género, Ocupación
   - **Contactos del Solicitante:** CRUD inline con Tipo (select: personal, work, emergency, other — labels UI: Personal, Laboral, Emergencia, Otro), Email, Código de teléfono, Teléfono
   - **Direcciones del Solicitante:** CRUD inline con Tipo (select: home, work, legal, other — labels UI: Hogar, Laboral, Legal, Otro), Calle, Número, Piso, Dpto, Provincia (select parametrizado), Ciudad (select en cascada), Código Postal
   - **Beneficiarios:** CRUD inline con nombre, apellido, parentesco y porcentaje
   - **Documentos:** Lista de documentos requeridos según el producto seleccionado

2. **DADO** que el usuario ingresa un tipo y número de documento en la sección Solicitante, **CUANDO** busca, **ENTONCES** si el solicitante ya existe se precargan todos sus datos (datos personales, contactos y direcciones) y se permite editarlos

3. **DADO** que el solicitante no existe, **CUANDO** el usuario completa los datos y guarda la solicitud, **ENTONCES** se crea el solicitante y se asocia a la solicitud

4. **DADO** que el usuario selecciona una entidad, **CUANDO** cambia, **ENTONCES** los productos se filtran a los disponibles para esa entidad

5. **DADO** que el usuario selecciona un producto, **CUANDO** cambia, **ENTONCES** los planes se filtran a los disponibles para ese producto

6. **DADO** que el usuario selecciona una provincia en una dirección, **CUANDO** cambia, **ENTONCES** las ciudades se filtran automáticamente por la provincia seleccionada (cascading select)

7. **DADO** que se edita una solicitud (`/solicitudes/:id/editar`), **CUANDO** carga, **ENTONCES** el formulario se precarga con todos los datos incluyendo el solicitante con sus contactos y direcciones

8. **DADO** que todos los campos requeridos están completos, **CUANDO** guarda, **ENTONCES** se genera un código automático (SOL-YYYY-NNN), se asigna estado `draft` y se redirige a la lista

## Campos del Formulario de Dirección

| Campo | Tipo UI | Fuente de datos |
|-------|---------|----------------|
| type | Select | Fijo: home, work, legal, other |
| street | Input text | — |
| number | Input text | — |
| floor | Input text (opcional) | — |
| apartment | Input text (opcional) | — |
| province | Select | Grupo `provinces` |
| city | Select | Grupo `cities` (filtrado por provincia) |
| postalCode | Input text | — |

## Campos del Formulario de Contacto

| Campo | Tipo UI | Fuente de datos |
|-------|---------|----------------|
| type | Select | Fijo: personal, work, emergency, other |
| email | Input email (opcional) | — |
| phoneCode | Input text (opcional) | — |
| phone | Input text (opcional) | — |

## Componentes Involucrados

- `src/pages/applications/ApplicationForm.tsx`
