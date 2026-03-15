# HU031 — Gestionar Requisitos de Producto

**Módulo:** Catálogo  
**Rol:** Super Admin, Admin Entidad, Admin Producto  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** administrador de productos  
**Quiero** definir los requisitos documentales de un producto  
**Para** especificar qué documentación necesita el solicitante para tramitar el producto

## Criterios de Aceptación

1. **DADO** que se edita un producto, **CUANDO** se muestra la sección de requisitos, **ENTONCES** se lista una grilla con:
   - Nombre del requisito
   - Tipo: Documento, Dato, Validación
   - Obligatorio (sí/no)
   - Descripción

2. **DADO** que presiona "Agregar Requisito", **CUANDO** completa los campos, **ENTONCES** se agrega a la lista

3. **DADO** que edita un requisito existente, **CUANDO** modifica los campos, **ENTONCES** se actualiza

4. **DADO** que elimina un requisito, **CUANDO** confirma, **ENTONCES** se quita de la lista

## Componentes Involucrados

- `src/pages/products/ProductForm.tsx` (sección requisitos)
