# HU015 — Listar Productos

**Módulo:** Catálogo  
**Rol:** Todos los roles con "Ver Productos"  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Productos"  
**Quiero** ver un listado de todos los productos financieros  
**Para** consultar el catálogo y gestionar los productos

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/productos`, **CUANDO** carga, **ENTONCES** se muestra una grilla con columnas:
   - Nombre
   - Código
   - Entidad
   - Familia
   - Estado (badge semántico)
   - Versión
   - Fecha de creación

2. **DADO** que hay productos, **CUANDO** busca, **ENTONCES** filtra por nombre y código

3. **DADO** que hace click en una fila, **CUANDO** navega, **ENTONCES** redirige a `/productos/:id`

4. **DADO** que exporta, **CUANDO** descarga, **ENTONCES** obtiene Excel o CSV

## Componentes Involucrados

- `src/pages/products/ProductList.tsx`
- `src/components/shared/DataTable.tsx`
