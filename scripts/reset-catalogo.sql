-- ============================================================================
-- Reset de catálogo (SOLO DESARROLLO)
-- ----------------------------------------------------------------------------
-- Vacía productos, categorías, tipos de lente y las ventas / trabajos a pedido /
-- movimientos de stock que los referencian. CONSERVA usuarios, pacientes,
-- profesionales, turnos, egresos y la caja general (solo se quitan los
-- movimientos de caja ligados a ventas).
--
-- Al reiniciar la API, los seeders repueblan el catálogo nuevo:
--   · DbSeeder      → categorías (con tipo), marcas, modelos, tipos de lente
--                     (con precio base), tratamientos.
--   · DevDataSeeder → productos de ejemplo con stock inicial.
--
-- Uso (con la API detenida):
--   psql "<cadena de conexión>" -f scripts/reset-catalogo.sql
-- o pegar el contenido en tu cliente SQL (DBeaver, pgAdmin…).
--
-- Nota de nombres: las tablas de inventario/compras usan PascalCase y van entre
-- comillas dobles ("Productos"); las demás son snake_case.
-- ============================================================================

BEGIN;

-- ── Ventas y su cadena ──────────────────────────────────────────────────────
DELETE FROM cobro_lineas;
DELETE FROM cobros;
DELETE FROM comprobantes;
DELETE FROM facturas_venta;
DELETE FROM devolucion_lineas;
DELETE FROM devoluciones;
DELETE FROM facturas_laboratorio;
DELETE FROM trabajos_pedido_tratamientos;
DELETE FROM trabajos_pedido;
DELETE FROM venta_lineas;
DELETE FROM movimientos_caja WHERE "VentaId" IS NOT NULL;  -- conserva egresos y caja general
DELETE FROM ventas;

-- ── Inventario / stock ──────────────────────────────────────────────────────
DELETE FROM "ConteoInventarioLineas";
DELETE FROM "ConteosInventario";
DELETE FROM "RecepcionesMercaderiaItems";
DELETE FROM "RecepcionesMercaderia";
DELETE FROM "PedidosProveedorItems";
DELETE FROM "PedidosProveedor";
DELETE FROM "FacturaCompraItems";
DELETE FROM "StockLotes";
DELETE FROM "MovimientosStock";
DELETE FROM productos_stock_config;
DELETE FROM "Productos";

-- ── Catálogo ────────────────────────────────────────────────────────────────
DELETE FROM categorias_producto;
DELETE FROM tipos_lente;

COMMIT;
