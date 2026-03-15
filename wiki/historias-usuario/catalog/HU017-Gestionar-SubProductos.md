# HU017 — Gestionar Sub Productos (Planes)

**Módulo:** Catálogo  
**Rol:** Super Admin, Admin Entidad, Admin Producto  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** administrador de productos  
**Quiero** crear, editar y eliminar sub productos (planes) dentro de un producto  
**Para** definir las variantes comerciales con sus atributos específicos según la categoría

## Criterios de Aceptación

### General

1. **DADO** que el usuario edita un producto, **CUANDO** observa la sección de planes, **ENTONCES** ve una grilla con los sub productos existentes

2. **DADO** que presiona "Agregar Plan", **CUANDO** se abre el diálogo modal (PlanDialog), **ENTONCES** muestra campos comunes + campos específicos según la categoría de la familia

### Campos Comunes

3. **DADO** que se crea un plan, **CUANDO** completa el formulario, **ENTONCES** los campos comunes son:
   - Nombre (requerido)
   - Descripción
   - Moneda: ARS / USD (requerido)
   - Cuotas (número)
   - Plan de comisiones (select, nullable — un plan de comisión puede asignarse a múltiples planes)
   - Precio (número, requerido) — mapea a `product_plans.price`
   - Estado (requerido)

### Atributos de Préstamo (familia con prefijo PREST)

4. **DADO** que la familia es de préstamos, **CUANDO** se muestra el formulario, **ENTONCES** aparecen:
   - Fila 1: Sistema de Amortización (select: Francés, Alemán, Americano, Bullet) | TEA %
   - Fila 2: CFT % | Gastos de Emisión ($)

### Atributos de Seguro (familia con prefijo SEG)

5. **DADO** que la familia es de seguros, **CUANDO** se muestra el formulario, **ENTONCES** aparecen:
   - Fila 1: Prima ($) | Suma Asegurada ($)
   - Fila 2: Días de Gracia | Tipo de Cobertura (Individual, Grupal, Colectiva)
   - Sección: CRUD inline de Coberturas (select parametrizado desde `insurance_coverages`, con campos Suma Asegurada $ y Prima $ por cobertura)

### Atributos de Cuenta (familia con prefijo CTA)

6. **DADO** que la familia es de cuentas, **CUANDO** se muestra, **ENTONCES** aparecen:
   - Fila 1: Comisión Mantenimiento ($) | Saldo Mínimo ($)
   - Fila 2: Tasa Interés Anual (%, max 3 chars) | Tipo de Cuenta (Caja de Ahorro, Cuenta Corriente, Mercado de Dinero)

### Atributos de Tarjeta (familia con prefijo TARJETA)

7. **DADO** que la familia es de tarjetas, **CUANDO** se muestra, **ENTONCES** aparecen:
   - Fila 1: Límite de Crédito ($) | Red (select parametrizado desde `card_networks`)
   - Fila 2: Cuota Anual ($) | Tasa Interés (%, max 3 chars)
   - Fila 3: Nivel (select parametrizado desde `card_levels`)

### Atributos de Inversión (familia con prefijo INV)

8. **DADO** que la familia es de inversiones, **CUANDO** se muestra, **ENTONCES** aparecen:
   - Fila 1: Monto Mínimo | Retorno Esperado %
   - Fila 2: Plazo (Días) | Riesgo (select: low, medium, high)

## Componentes Involucrados

- `src/pages/products/ProductForm.tsx` (PlanDialog)
