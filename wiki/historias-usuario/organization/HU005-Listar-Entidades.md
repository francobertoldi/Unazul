# HU005 — Listar Entidades

**Módulo:** Organización  
**Rol:** Todos los roles con "Ver Entidades"  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Entidades"  
**Quiero** ver un listado de todas las entidades financieras del sistema  
**Para** gestionar y consultar las organizaciones financieras registradas

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/entidades`, **CUANDO** la página carga, **ENTONCES** se muestra una grilla con columnas:
   - Nombre
   - Identificador (CUIT)
   - Organización (nombre de la organización padre)
   - Tipo (bank, insurance, fintech, cooperative, sgr, regional_card)
   - Estado (badge con color semántico)
   - Fecha de Creación

2. **DADO** que hay entidades, **CUANDO** el usuario busca, **ENTONCES** se filtra por nombre e identificador

3. **DADO** que el usuario hace click en una fila, **CUANDO** navega, **ENTONCES** se redirige a `/entidades/:id`

4. **DADO** que el usuario usa las acciones de fila, **CUANDO** presiona editar, **ENTONCES** navega a `/entidades/:id/editar`

5. **DADO** que el usuario presiona eliminar, **CUANDO** confirma, **ENTONCES** la entidad se elimina con notificación

6. **DADO** que hay datos, **CUANDO** exporta, **ENTONCES** puede descargar en Excel o CSV

## Componentes Involucrados

- `src/pages/entities/EntityList.tsx`
- `src/components/shared/DataTable.tsx`
