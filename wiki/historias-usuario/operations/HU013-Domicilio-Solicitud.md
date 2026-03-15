# HU013 — Direcciones del Solicitante

**Módulo:** Operaciones
**Rol:** Super Admin, Admin Entidad, Operador
**Prioridad:** Media
**Estado:** Implementada

---

## Descripción

**Como** operador
**Quiero** registrar y visualizar las direcciones del solicitante
**Para** tener la información de ubicación completa con georreferenciación en mapa

## Criterios de Aceptación

### Formulario (Edición)

1. **DADO** que el usuario crea o edita una solicitud, **CUANDO** completa la sección "Direcciones del Solicitante", **ENTONCES** puede agregar múltiples direcciones con:
   - Tipo de dirección (select: hogar/laboral/legal/otro)
   - Calle (input text)
   - Número (input text)
   - Piso (input text, opcional)
   - Departamento (input text, opcional)
   - Provincia (select parametrizado desde grupo `provinces`)
   - Ciudad (select en cascada desde grupo `cities`, filtrado por provincia seleccionada)
   - Código Postal (input text)

2. **DADO** que el usuario selecciona una provincia, **CUANDO** cambia el valor, **ENTONCES** el combo de ciudades se vacía y se recarga con las ciudades de la provincia seleccionada (filtro por `parentKey`)

3. **DADO** que el usuario cambia la provincia, **CUANDO** ya tenía una ciudad seleccionada, **ENTONCES** la ciudad se limpia automáticamente

4. **DADO** que el solicitante ya existe y tiene direcciones registradas, **CUANDO** se vincula al buscarlo por documento, **ENTONCES** sus direcciones (que pertenecen al solicitante, no a la solicitud) se precargan para visualización y edición. **Nota:** las direcciones se persisten en `applicant_addresses` bajo `applicant_id`, nunca bajo `application_id`

### Visualización (Detalle)

5. **DADO** que el solicitante tiene direcciones, **CUANDO** el usuario selecciona la solapa "Direcciones", **ENTONCES** ve una lista donde cada dirección muestra:
   - Badge con tipo de dirección
   - Encabezado: ícono MapPin + tipo de dirección
   - Fila 1: Calle (50%) | Número + Piso + Dpto (3 subcampos, 50%)
   - Fila 2: Provincia | Ciudad | Código Postal (3 columnas iguales)
   - Los campos opcionales vacíos muestran "—"

6. **DADO** que se muestra una dirección, **CUANDO** se renderiza el mapa, **ENTONCES** se muestra un iframe de Google Maps con:
   - Georreferenciación automática: query = "{calle} {número}, {ciudad}, {provincia}, Argentina"
   - Ancho: 100%
   - Alto: 300px
   - Bordes redondeados con overflow hidden
   - Carga lazy (loading="lazy")

7. **DADO** que el solicitante NO tiene direcciones, **CUANDO** selecciona la solapa, **ENTONCES** muestra "No se han registrado direcciones para este solicitante."

## Componentes Involucrados

- `src/pages/applications/ApplicationDetail.tsx` (solapa Direcciones)
- `src/pages/applications/ApplicationForm.tsx` (sección Direcciones)
- `src/data/types.ts` (interfaz ApplicantAddress)
