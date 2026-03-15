# HU034 — Panel de Atributos en Editor de Workflows

**Módulo:** Configuración  
**Rol:** Diseñadores de procesos  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** diseñador de procesos  
**Quiero** tener un panel con todos los atributos del dominio disponibles en el editor de workflows  
**Para** poder usarlos en fórmulas y condiciones de decisión del flujo

## Criterios de Aceptación

1. **DADO** que el usuario está en el editor de workflows, **CUANDO** observa el lado derecho, **ENTONCES** se muestra un panel lateral con el catálogo de atributos agrupados por objeto.

2. **DADO** que se muestran los atributos, **CUANDO** el usuario expande un grupo, **ENTONCES** cada atributo muestra:
   - Nombre descriptivo
   - Ruta completa (`Objeto.atributo`)
   - Tipo de dato con badge de color (string=azul, number=verde, date=ámbar, enum=rosa)

3. **DADO** que los atributos son arrastrables, **CUANDO** el usuario arrastra un atributo, **ENTONCES** se genera una referencia `{{Objeto.atributo}}` en formato texto.

4. **DADO** que el usuario edita un nodo de tipo Decisión (doble click), **CUANDO** se abre el diálogo de configuración, **ENTONCES** se muestra un panel de atributos a la derecha del campo "Condición".

5. **DADO** que el usuario hace click en un atributo del panel de Decisión, **CUANDO** el campo "Condición" tiene foco, **ENTONCES** se inserta `{{Objeto.atributo}}` en la posición del cursor.

6. **DADO** que el catálogo incluye el objeto Persona (Solicitante), **CUANDO** se expande, **ENTONCES** incluye los atributos: firstName, lastName, documentType, documentNumber, email, phone, birthDate, age (calculada), gender, occupation.

## Objetos del Catálogo

| Objeto | Cantidad de Atributos |
|--------|:---------------------:|
| Persona (Solicitante) | 10 |
| Domicilio | 9 |
| Solicitud | 9 |
| Producto | 7 |
| SubProducto (Plan) | 6 |
| SubProducto — Préstamo | 4 |
| SubProducto — Seguro | 4 |
| SubProducto — Tarjeta | 6 |
| SubProducto — Cuenta | 4 |
| SubProducto — Inversión | 5 |
| Cobertura (Seguro) | 4 |

## Componentes Involucrados

- `src/pages/workflows/WorkflowEditor.tsx`
- `src/components/workflow/NodeConfigDialog.tsx`

## Datos de Entrada

Catálogo estático de atributos del dominio definido en los componentes.

## Datos de Salida

Panel visual con atributos arrastrables e insertables en condiciones de decisión.
