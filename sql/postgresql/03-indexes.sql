-- ============================================================
-- Inventory App - Additional Indexes
-- PostgreSQL / InventoryAppDB
-- ============================================================

-- Movement: query kartu stok dan laporan per lokasi/tanggal
CREATE INDEX IF NOT EXISTS "IX_StockMovements_LocationId_CreatedAt"
    ON "StockMovements" ("LocationId", "CreatedAt" DESC);

CREATE INDEX IF NOT EXISTS "IX_StockMovements_ProductId_CreatedAt"
    ON "StockMovements" ("ProductId", "CreatedAt" DESC);

CREATE INDEX IF NOT EXISTS "IX_StockMovements_Reference"
    ON "StockMovements" ("ReferenceType", "ReferenceId");

-- Header transaksi: filter tanggal dan approver
CREATE INDEX IF NOT EXISTS "IX_StockInHeaders_Date"
    ON "StockInHeaders" ("Date" DESC);

CREATE INDEX IF NOT EXISTS "IX_StockInHeaders_ApproverId"
    ON "StockInHeaders" ("ApproverId")
    WHERE "ApproverId" IS NOT NULL;

CREATE INDEX IF NOT EXISTS "IX_StockOutHeaders_Date"
    ON "StockOutHeaders" ("Date" DESC);

CREATE INDEX IF NOT EXISTS "IX_StockOutHeaders_ApproverId"
    ON "StockOutHeaders" ("ApproverId")
    WHERE "ApproverId" IS NOT NULL;

CREATE INDEX IF NOT EXISTS "IX_StockTransferHeaders_Date"
    ON "StockTransferHeaders" ("Date" DESC);

CREATE INDEX IF NOT EXISTS "IX_StockTransferHeaders_ApproverId"
    ON "StockTransferHeaders" ("ApproverId")
    WHERE "ApproverId" IS NOT NULL;

CREATE INDEX IF NOT EXISTS "IX_StockAdjustmentHeaders_Date"
    ON "StockAdjustmentHeaders" ("Date" DESC);

CREATE INDEX IF NOT EXISTS "IX_StockAdjustmentHeaders_ApproverId"
    ON "StockAdjustmentHeaders" ("ApproverId")
    WHERE "ApproverId" IS NOT NULL;

CREATE INDEX IF NOT EXISTS "IX_StockOpnameHeaders_Date"
    ON "StockOpnameHeaders" ("Date" DESC);

CREATE INDEX IF NOT EXISTS "IX_StockOpnameHeaders_ApproverId"
    ON "StockOpnameHeaders" ("ApproverId")
    WHERE "ApproverId" IS NOT NULL;

-- Audit log: filter entity/action/tanggal
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Entity_Action_CreatedAt"
    ON "AuditLogs" ("EntityName", "Action", "CreatedAt" DESC);

-- Partial index untuk opsi master yang aktif
CREATE INDEX IF NOT EXISTS "IX_Products_Active"
    ON "Products" ("IsActive")
    WHERE "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Suppliers_Active"
    ON "Suppliers" ("IsActive")
    WHERE "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Locations_Active"
    ON "Locations" ("IsActive")
    WHERE "IsDeleted" = false;
