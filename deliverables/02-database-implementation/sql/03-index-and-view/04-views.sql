-- ============================================================
-- Inventory App - Views
-- PostgreSQL / InventoryAppDB
-- ============================================================

-- View sederhana (auto-updatable): dipakai contoh DML melalui view
CREATE OR REPLACE VIEW "v_product_basic" AS
SELECT "Id", "Sku", "Name", "CategoryId", "UnitId", "ReorderLevel", "IsActive", "CreatedAt", "IsDeleted"
FROM "Products"
WHERE "IsDeleted" = false;

-- Saldo stok per produk/lokasi
CREATE OR REPLACE VIEW "v_stock_balance" AS
SELECT
    s."Id"               AS "StockId",
    p."Id"               AS "ProductId",
    p."Sku"              AS "Sku",
    p."Name"             AS "ProductName",
    u."Name"             AS "UnitName",
    l."Id"               AS "LocationId",
    l."Code"             AS "LocationCode",
    l."Name"             AS "LocationName",
    l."BranchCode"       AS "BranchCode",
    s."Quantity"          AS "Quantity",
    p."ReorderLevel"     AS "ReorderLevel",
    s."UpdatedAt"         AS "UpdatedAt"
FROM "Stocks" s
JOIN "Products" p ON p."Id" = s."ProductId"
JOIN "Units" u ON u."Id" = p."UnitId"
JOIN "Locations" l ON l."Id" = s."LocationId"
WHERE p."IsDeleted" = false
  AND l."IsDeleted" = false;

-- Produk yang menyentuh atau di bawah reorder level
CREATE OR REPLACE VIEW "v_reorder_alert" AS
SELECT *
FROM "v_stock_balance"
WHERE "Quantity" <= "ReorderLevel";

-- Pending approval dari lima tipe transaksi
CREATE OR REPLACE VIEW "v_pending_approvals" AS
SELECT "Id", 'StockIn'::text AS "Type", "DocNo", "Date", "LocationId", "ApproverId", "CreatedById", "SubmittedAt"
FROM "StockInHeaders"
WHERE "Status" = 1 AND "IsDeleted" = false
UNION ALL
SELECT "Id", 'StockOut'::text, "DocNo", "Date", "LocationId", "ApproverId", "CreatedById", "SubmittedAt"
FROM "StockOutHeaders"
WHERE "Status" = 1 AND "IsDeleted" = false
UNION ALL
SELECT "Id", 'StockTransfer'::text, "DocNo", "Date", "FromLocationId", "ApproverId", "CreatedById", "SubmittedAt"
FROM "StockTransferHeaders"
WHERE "Status" = 1 AND "IsDeleted" = false
UNION ALL
SELECT "Id", 'StockAdjustment'::text, "DocNo", "Date", "LocationId", "ApproverId", "CreatedById", "SubmittedAt"
FROM "StockAdjustmentHeaders"
WHERE "Status" = 1 AND "IsDeleted" = false
UNION ALL
SELECT "Id", 'StockOpname'::text, "DocNo", "Date", "LocationId", "ApproverId", "CreatedById", "SubmittedAt"
FROM "StockOpnameHeaders"
WHERE "Status" = 1 AND "IsDeleted" = false;

-- Rekap mutasi harian per produk/lokasi
CREATE OR REPLACE VIEW "v_stock_movement_daily" AS
SELECT
    date_trunc('day', m."CreatedAt")::date AS "MovementDate",
    p."Sku",
    p."Name" AS "ProductName",
    l."Name" AS "LocationName",
    m."Type",
    SUM(m."Quantity") AS "TotalQuantity",
    COUNT(*) AS "MovementCount"
FROM "StockMovements" m
JOIN "Products" p ON p."Id" = m."ProductId"
JOIN "Locations" l ON l."Id" = m."LocationId"
GROUP BY date_trunc('day', m."CreatedAt")::date, p."Sku", p."Name", l."Name", m."Type";

-- Kartu stok dengan saldo berjalan
CREATE OR REPLACE VIEW "v_stock_card" AS
SELECT
    m."Id" AS "MovementId",
    m."ProductId",
    p."Sku",
    p."Name" AS "ProductName",
    l."Name" AS "LocationName",
    m."MovementNo",
    m."Type",
    m."Quantity",
    SUM(m."Quantity") OVER (
        PARTITION BY m."ProductId", m."LocationId"
        ORDER BY m."CreatedAt", m."Id"
        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    ) AS "RunningBalance",
    m."Notes",
    m."CreatedAt"
FROM "StockMovements" m
JOIN "Products" p ON p."Id" = m."ProductId"
JOIN "Locations" l ON l."Id" = m."LocationId"
WHERE p."IsDeleted" = false
  AND l."IsDeleted" = false;

-- Rekap audit per entitas/aksi
CREATE OR REPLACE VIEW "v_audit_summary" AS
SELECT
    "EntityName",
    "Action",
    COUNT(*) AS "Total",
    MIN("CreatedAt") AS "FirstAt",
    MAX("CreatedAt") AS "LastAt"
FROM "AuditLogs"
GROUP BY "EntityName", "Action";
