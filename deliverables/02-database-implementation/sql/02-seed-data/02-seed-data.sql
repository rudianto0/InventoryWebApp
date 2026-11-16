-- ============================================================
-- Inventory App - Seed Data (DML)
-- Target: PostgreSQL, database InventoryAppDB
-- Aman dijalankan berulang (idempotent) dengan ON CONFLICT.
-- ============================================================

-- ------------------------------------------------------------
-- 1. Master: Categories, Units, Suppliers
-- ------------------------------------------------------------
INSERT INTO "Categories" ("Id", "Name", "Description", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    ('10000000-0000-0000-0000-000000000001', 'General', 'General purpose items', true, now(), false),
    ('10000000-0000-0000-0000-000000000002', 'Consumable', 'Consumable items', true, now(), false)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Units" ("Id", "Code", "Name", "CreatedAt", "IsDeleted")
VALUES
    ('20000000-0000-0000-0000-000000000001', 'PCS', 'Pieces', now(), false),
    ('20000000-0000-0000-0000-000000000002', 'BOX', 'Box', now(), false)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Suppliers" ("Id", "Code", "Name", "ContactPerson", "Phone", "Email", "Address", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    ('30000000-0000-0000-0000-000000000001', 'SUP-001', 'PT Sumber Makmur', 'Budi', '021-5551234', 'sales@sumbermakmur.local', 'Jakarta', true, now(), false),
    ('30000000-0000-0000-0000-000000000002', 'SUP-002', 'CV Cahaya Office', 'Sari', '021-5559876', 'sales@cahaya.local', 'Bandung', true, now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 2. Locations (Branch -> Warehouse)
-- Type: Branch = 0, Warehouse = 1, Store = 2
-- ------------------------------------------------------------
INSERT INTO "Locations" ("Id", "Code", "Name", "Type", "ParentLocationId", "BranchCode", "Address", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    ('40000000-0000-0000-0000-000000000001', 'HQ', 'Kantor Pusat', 0, NULL, 'HQ', 'Jakarta', true, now(), false),
    ('40000000-0000-0000-0000-000000000002', 'HQ-WH1', 'Gudang Pusat', 1, '40000000-0000-0000-0000-000000000001', 'HQ', 'Jakarta', true, now(), false),
    ('40000000-0000-0000-0000-000000000003', 'BR2', 'Cabang Bandung', 0, NULL, 'BR2', 'Bandung', true, now(), false),
    ('40000000-0000-0000-0000-000000000004', 'BR2-WH1', 'Gudang Bandung', 1, '40000000-0000-0000-0000-000000000003', 'BR2', 'Bandung', true, now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 3. Products
-- ------------------------------------------------------------
INSERT INTO "Products" ("Id", "Sku", "Name", "CategoryId", "UnitId", "ReorderLevel", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    ('50000000-0000-0000-0000-000000000001', 'SKU-001', 'A4 Paper Ream', '10000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', 10, true, now(), false),
    ('50000000-0000-0000-0000-000000000002', 'SKU-002', 'Printer Toner', '10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000001', 5, true, now(), false),
    ('50000000-0000-0000-0000-000000000003', 'SKU-003', 'Archived Box', '10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000002', 3, true, now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 4. Stocks (saldo per produk/lokasi)
-- ------------------------------------------------------------
INSERT INTO "Stocks" ("Id", "ProductId", "LocationId", "Quantity", "UpdatedAt", "Version", "CreatedAt")
VALUES
    ('60000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', 70, now(), 1, now()),
    ('60000000-0000-0000-0000-000000000002', '50000000-0000-0000-0000-000000000002', '40000000-0000-0000-0000-000000000002', 18, now(), 1, now()),
    ('60000000-0000-0000-0000-000000000003', '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000004', 58, now(), 1, now()),
    ('60000000-0000-0000-0000-000000000004', '50000000-0000-0000-0000-000000000003', '40000000-0000-0000-0000-000000000004', 30, now(), 1, now())
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 5. Stock In (2 dokumen approved)
-- Status: Draft=0, Submitted=1, Approved=2, Rejected=3, Cancelled=4
-- ------------------------------------------------------------
INSERT INTO "StockInHeaders" ("Id", "DocNo", "SupplierId", "LocationId", "Date", "Status", "SubmittedAt", "ApproverId", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
VALUES
    ('70000000-0000-0000-0000-000000000001', 'IN-202611-0001', '30000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', now() - interval '30 days', 2, now() - interval '30 days', 2, now() - interval '29 days', 2, now() - interval '30 days', 1, false, 'Penerimaan awal'),
    ('70000000-0000-0000-0000-000000000002', 'IN-202611-0002', '30000000-0000-0000-0000-000000000002', '40000000-0000-0000-0000-000000000004', now() - interval '20 days', 2, now() - interval '20 days', 2, now() - interval '19 days', 2, now() - interval '20 days', 1, false, 'Stok cabang')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "StockInDetails" ("Id", "StockInHeaderId", "ProductId", "Quantity", "UnitCost", "Notes", "CreatedAt", "IsDeleted")
VALUES
    ('71000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', 100, 45000.0000, 'A4', now(), false),
    ('71000000-0000-0000-0000-000000000002', '70000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000002', 20, 250000.0000, 'Toner', now(), false),
    ('71000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000002', '50000000-0000-0000-0000-000000000001', 50, 46000.0000, 'A4 cabang', now(), false),
    ('71000000-0000-0000-0000-000000000004', '70000000-0000-0000-0000-000000000002', '50000000-0000-0000-0000-000000000003', 30, 15000.0000, 'Box', now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 6. Stock Out (approved)
-- ------------------------------------------------------------
INSERT INTO "StockOutHeaders" ("Id", "DocNo", "LocationId", "Date", "Purpose", "Status", "SubmittedAt", "ApproverId", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
VALUES
    ('72000000-0000-0000-0000-000000000001', 'OUT-202611-0001', '40000000-0000-0000-0000-000000000002', now() - interval '10 days', 'Pemakaian kantor', 2, now() - interval '10 days', 2, now() - interval '9 days', 2, now() - interval '10 days', 1, false, 'Kebutuhan operasional')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "StockOutDetails" ("Id", "StockOutHeaderId", "ProductId", "Quantity", "Notes", "CreatedAt", "IsDeleted")
VALUES
    ('72100000-0000-0000-0000-000000000001', '72000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', 15, 'A4', now(), false),
    ('72100000-0000-0000-0000-000000000002', '72000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000002', 2, 'Toner', now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 7. Stock Transfer (approved)
-- ------------------------------------------------------------
INSERT INTO "StockTransferHeaders" ("Id", "DocNo", "FromLocationId", "ToLocationId", "Date", "Status", "SubmittedAt", "ApproverId", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
VALUES
    ('73000000-0000-0000-0000-000000000001', 'TRF-202611-0001', '40000000-0000-0000-0000-000000000002', '40000000-0000-0000-0000-000000000004', now() - interval '8 days', 2, now() - interval '8 days', 2, now() - interval '7 days', 2, now() - interval '8 days', 1, false, 'Distribusi ke cabang')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "StockTransferDetails" ("Id", "StockTransferHeaderId", "ProductId", "Quantity", "Notes", "CreatedAt", "IsDeleted")
VALUES
    ('73100000-0000-0000-0000-000000000001', '73000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', 10, 'A4', now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 8. Stock Adjustment (approved)
-- ------------------------------------------------------------
INSERT INTO "StockAdjustmentHeaders" ("Id", "DocNo", "LocationId", "Date", "Reason", "Status", "SubmittedAt", "ApproverId", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
VALUES
    ('74000000-0000-0000-0000-000000000001', 'ADJ-202611-0001', '40000000-0000-0000-0000-000000000002', now() - interval '5 days', 'Koreksi stok', 2, now() - interval '5 days', 2, now() - interval '4 days', 2, now() - interval '5 days', 1, false, 'Selisih hitung')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "StockAdjustmentDetails" ("Id", "StockAdjustmentHeaderId", "ProductId", "SystemQuantity", "CountedQuantity", "Difference", "Notes", "CreatedAt", "IsDeleted")
VALUES
    ('74100000-0000-0000-0000-000000000001', '74000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', 75, 70, -5, 'A4 selisih', now(), false),
    ('74100000-0000-0000-0000-000000000002', '74000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000002', 18, 18, 0, 'Toner sesuai', now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 9. Stock Opname (approved)
-- ------------------------------------------------------------
INSERT INTO "StockOpnameHeaders" ("Id", "DocNo", "LocationId", "Date", "Status", "SubmittedAt", "ApproverId", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
VALUES
    ('75000000-0000-0000-0000-000000000001', 'OPN-202611-0001', '40000000-0000-0000-0000-000000000004', now() - interval '3 days', 2, now() - interval '3 days', 2, now() - interval '2 days', 2, now() - interval '3 days', 1, false, 'Opname cabang')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "StockOpnameDetails" ("Id", "StockOpnameHeaderId", "ProductId", "SystemQuantity", "CountedQuantity", "Difference", "Notes", "CreatedAt", "IsDeleted")
VALUES
    ('75100000-0000-0000-0000-000000000001', '75000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', 60, 58, -2, 'A4 cabang', now(), false),
    ('75100000-0000-0000-0000-000000000002', '75000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000003', 30, 30, 0, 'Box sesuai', now(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 10. Stock Movements
-- Type: In=0, Out=1, TransferOut=2, TransferIn=3, Adjustment=4, Opname=5
-- ReferenceType: StockIn=0, StockOut=1, StockTransfer=2, StockAdjustment=3, StockOpname=4
-- ------------------------------------------------------------
INSERT INTO "StockMovements" ("Id", "MovementNo", "Type", "ProductId", "LocationId", "Quantity", "ReferenceType", "ReferenceId", "Notes", "CreatedAt", "CreatedById")
VALUES
    ('80000000-0000-0000-0000-000000000001', 'MOV-202611-0001', 0, '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', 100, 0, '70000000-0000-0000-0000-000000000001', 'Stock In', now() - interval '30 days', 1),
    ('80000000-0000-0000-0000-000000000002', 'MOV-202611-0002', 0, '50000000-0000-0000-0000-000000000002', '40000000-0000-0000-0000-000000000002', 20, 0, '70000000-0000-0000-0000-000000000001', 'Stock In', now() - interval '30 days', 1),
    ('80000000-0000-0000-0000-000000000003', 'MOV-202611-0003', 0, '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000004', 50, 0, '70000000-0000-0000-0000-000000000002', 'Stock In', now() - interval '20 days', 1),
    ('80000000-0000-0000-0000-000000000004', 'MOV-202611-0004', 0, '50000000-0000-0000-0000-000000000003', '40000000-0000-0000-0000-000000000004', 30, 0, '70000000-0000-0000-0000-000000000002', 'Stock In', now() - interval '20 days', 1),
    ('80000000-0000-0000-0000-000000000005', 'MOV-202611-0005', 1, '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', -15, 1, '72000000-0000-0000-0000-000000000001', 'Stock Out', now() - interval '10 days', 1),
    ('80000000-0000-0000-0000-000000000006', 'MOV-202611-0006', 1, '50000000-0000-0000-0000-000000000002', '40000000-0000-0000-0000-000000000002', -2, 1, '72000000-0000-0000-0000-000000000001', 'Stock Out', now() - interval '10 days', 1),
    ('80000000-0000-0000-0000-000000000007', 'MOV-202611-0007', 2, '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', -10, 2, '73000000-0000-0000-0000-000000000001', 'Transfer out', now() - interval '8 days', 1),
    ('80000000-0000-0000-0000-000000000008', 'MOV-202611-0008', 3, '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000004', 10, 2, '73000000-0000-0000-0000-000000000001', 'Transfer in', now() - interval '8 days', 1),
    ('80000000-0000-0000-0000-000000000009', 'MOV-202611-0009', 4, '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', -5, 3, '74000000-0000-0000-0000-000000000001', 'Adjustment', now() - interval '5 days', 1),
    ('80000000-0000-0000-0000-000000000010', 'MOV-202611-0010', 5, '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000004', -2, 4, '75000000-0000-0000-0000-000000000001', 'Opname', now() - interval '3 days', 1)
ON CONFLICT ("Id") DO NOTHING;

-- ------------------------------------------------------------
-- 11. Document Sequences
-- ------------------------------------------------------------
INSERT INTO "DocumentSequences" ("Prefix", "Year", "Month", "LastNumber")
VALUES
    ('IN', 2026, 11, 2),
    ('OUT', 2026, 11, 1),
    ('TRF', 2026, 11, 1),
    ('ADJ', 2026, 11, 1),
    ('OPN', 2026, 11, 1)
ON CONFLICT ("Prefix", "Year", "Month") DO NOTHING;

-- ------------------------------------------------------------
-- 12. Audit log contoh (tidak termasuk tabel yang diaudit aplikasi)
-- ------------------------------------------------------------
INSERT INTO "AuditLogs" ("EntityName", "EntityId", "Action", "OldValues", "NewValues", "UserId", "IpAddress", "CreatedAt")
SELECT 'Product', '50000000-0000-0000-0000-000000000001', 'Create', NULL,
       '{"Sku":"SKU-001","Name":"A4 Paper Ream"}', 1, '127.0.0.1', now()
WHERE NOT EXISTS (
    SELECT 1 FROM "AuditLogs"
    WHERE "EntityName" = 'Product'
      AND "EntityId" = '50000000-0000-0000-0000-000000000001'
      AND "Action" = 'Create'
);
