-- ============================================================
-- Inventory App - Relational DML
-- PostgreSQL / InventoryAppDB
-- ============================================================

-- ------------------------------------------------------------
-- 4.1 / 4.2  DML dan query antar tabel
-- ------------------------------------------------------------

-- JOIN: produk + kategori + unit + saldo + lokasi
SELECT
    p."Sku",
    p."Name" AS "Product",
    c."Name" AS "Category",
    u."Code" AS "Unit",
    l."Name" AS "Location",
    s."Quantity",
    p."ReorderLevel"
FROM "Products" p
JOIN "Categories" c ON c."Id" = p."CategoryId"
JOIN "Units" u ON u."Id" = p."UnitId"
LEFT JOIN "Stocks" s ON s."ProductId" = p."Id"
LEFT JOIN "Locations" l ON l."Id" = s."LocationId"
ORDER BY p."Sku", l."Name";

-- Subquery: produk yang belum punya baris stok sama sekali
SELECT p."Sku", p."Name"
FROM "Products" p
WHERE NOT EXISTS (
    SELECT 1 FROM "Stocks" s WHERE s."ProductId" = p."Id"
);

-- CTE + agregasi: total mutasi per produk
WITH movement AS (
    SELECT "ProductId", SUM("Quantity") AS "NetQuantity", COUNT(*) AS "MovementCount"
    FROM "StockMovements"
    GROUP BY "ProductId"
)
SELECT p."Sku", p."Name", m."NetQuantity", m."MovementCount"
FROM movement m
JOIN "Products" p ON p."Id" = m."ProductId"
ORDER BY m."NetQuantity" DESC;

-- INSERT ... SELECT: snapshot reorder alert ke tabel sementara
DROP TABLE IF EXISTS "tmp_reorder_snapshot";
CREATE TEMP TABLE "tmp_reorder_snapshot" AS
SELECT "ProductId", "Sku", "ProductName", "LocationName", "Quantity", "ReorderLevel"
FROM "v_reorder_alert";

SELECT * FROM "tmp_reorder_snapshot" ORDER BY "Quantity";

-- UPDATE ... FROM: contoh sinkronisasi reorder level dari kategori
UPDATE "Products" p
SET "ReorderLevel" = x."NewLevel"
FROM (
    SELECT "CategoryId", CEIL(AVG("ReorderLevel"))::int AS "NewLevel"
    FROM "Products"
    WHERE "IsDeleted" = false
    GROUP BY "CategoryId"
) x
WHERE p."CategoryId" = x."CategoryId"
  AND p."Sku" = 'SKU-001';

-- DELETE ... USING: hapus data demo yang tidak dipakai transaksi
DELETE FROM "Products" p
USING "Categories" c
WHERE p."CategoryId" = c."Id"
  AND p."Sku" = 'SKU-DEMO-DELETE';

-- ------------------------------------------------------------
-- 4.3  DML melalui view sederhana (auto-updatable)
-- ------------------------------------------------------------

-- INSERT melalui view
INSERT INTO "v_product_basic"
    ("Id", "Sku", "Name", "CategoryId", "UnitId", "ReorderLevel", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    ('51000000-0000-0000-0000-000000000099', 'SKU-VIEW-01', 'Produk via View',
     '10000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001',
     4, true, now(), false);

-- UPDATE melalui view
UPDATE "v_product_basic"
SET "ReorderLevel" = 6, "Name" = 'Produk via View (updated)'
WHERE "Sku" = 'SKU-VIEW-01';

SELECT * FROM "v_product_basic" WHERE "Sku" = 'SKU-VIEW-01';

-- DELETE melalui view
DELETE FROM "v_product_basic" WHERE "Sku" = 'SKU-VIEW-01';

-- View kompleks bersifat read-only (hanya SELECT)
SELECT * FROM "v_pending_approvals" ORDER BY "SubmittedAt";
SELECT * FROM "v_stock_card" ORDER BY "CreatedAt" DESC LIMIT 20;

-- ------------------------------------------------------------
-- 4.4  DML efisien
-- ------------------------------------------------------------

-- EXISTS lebih aman/efisien dibanding IN untuk korelasi
SELECT p."Sku", p."Name"
FROM "Products" p
WHERE EXISTS (
    SELECT 1 FROM "StockMovements" m WHERE m."ProductId" = p."Id"
);

-- Batch insert idempotent
INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    ('30000000-0000-0000-0000-000000000099', 'SUP-DEMO', 'Supplier Demo', true, now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Hapus data demo
DELETE FROM "Suppliers" WHERE "Code" = 'SUP-DEMO';

-- Rencana eksekusi query laporan
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM "v_stock_balance" WHERE "Quantity" <= "ReorderLevel";
