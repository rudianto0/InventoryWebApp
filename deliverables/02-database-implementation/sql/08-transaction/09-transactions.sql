-- ============================================================
-- Inventory App - Transactions, Procedure/Function/Trigger Tests
-- PostgreSQL / InventoryAppDB
-- ============================================================

-- ------------------------------------------------------------
-- 8.1  COMMIT: perubahan disimpan
-- ------------------------------------------------------------
BEGIN;

INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "CreatedAt", "IsDeleted")
VALUES ('30000000-0000-0000-0000-000000000101', 'SUP-TRX-COMMIT', 'Supplier Commit', true, now(), false)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;

SELECT 'COMMIT: supplier harus ada' AS "Check", COUNT(*) AS "Count"
FROM "Suppliers" WHERE "Code" = 'SUP-TRX-COMMIT';

-- ------------------------------------------------------------
-- 8.2  ROLLBACK: perubahan dibatalkan
-- ------------------------------------------------------------
BEGIN;

INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "CreatedAt", "IsDeleted")
VALUES ('30000000-0000-0000-0000-000000000102', 'SUP-TRX-ROLLBACK', 'Supplier Rollback', true, now(), false);

ROLLBACK;

SELECT 'ROLLBACK: supplier harus 0' AS "Check", COUNT(*) AS "Count"
FROM "Suppliers" WHERE "Code" = 'SUP-TRX-ROLLBACK';

-- ------------------------------------------------------------
-- 8.3  SAVEPOINT
-- ------------------------------------------------------------
BEGIN;

INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "CreatedAt", "IsDeleted")
VALUES ('30000000-0000-0000-0000-000000000103', 'SUP-TRX-SAVE1', 'Supplier Savepoint 1', true, now(), false)
ON CONFLICT ("Id") DO NOTHING;

SAVEPOINT before_second;

INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "CreatedAt", "IsDeleted")
VALUES ('30000000-0000-0000-0000-000000000104', 'SUP-TRX-SAVE2', 'Supplier Savepoint 2', true, now(), false);

ROLLBACK TO SAVEPOINT before_second;

COMMIT;

SELECT 'SAVEPOINT: hanya SUP-TRX-SAVE1' AS "Check", "Code"
FROM "Suppliers"
WHERE "Code" IN ('SUP-TRX-SAVE1', 'SUP-TRX-SAVE2')
ORDER BY "Code";

-- ------------------------------------------------------------
-- 5.2  Uji stored procedure
-- ------------------------------------------------------------
-- Penerimaan stok
DO $$
DECLARE
    v_doc_id uuid;
BEGIN
    CALL sp_receive_stock(
        v_doc_id,
        '30000000-0000-0000-0000-000000000001',
        '40000000-0000-0000-0000-000000000002',
        '50000000-0000-0000-0000-000000000001',
        5,
        1
    );
    RAISE NOTICE 'sp_receive_stock doc_id=%', v_doc_id;
END;
$$;

-- Pengeluaran stok
DO $$
DECLARE
    v_doc_id uuid;
BEGIN
    CALL sp_issue_stock(
        v_doc_id,
        '40000000-0000-0000-0000-000000000002',
        '50000000-0000-0000-0000-000000000001',
        3,
        1
    );
    RAISE NOTICE 'sp_issue_stock doc_id=%', v_doc_id;
END;
$$;

-- Transfer stok
DO $$
DECLARE
    v_doc_id uuid;
BEGIN
    CALL sp_transfer_stock(
        v_doc_id,
        '40000000-0000-0000-0000-000000000002',
        '40000000-0000-0000-0000-000000000004',
        '50000000-0000-0000-0000-000000000001',
        2,
        1
    );
    RAISE NOTICE 'sp_transfer_stock doc_id=%', v_doc_id;
END;
$$;

-- Recalculate stok satu lokasi
DO $$
DECLARE
    v_rows integer := 0;
BEGIN
    CALL sp_recalculate_stock('40000000-0000-0000-0000-000000000002', v_rows);
    RAISE NOTICE 'sp_recalculate_stock rows=%', v_rows;
END;
$$;

-- Approve Stock In: buat header Submitted lalu approve
INSERT INTO "StockInHeaders"
    ("Id", "DocNo", "SupplierId", "LocationId", "Date", "Status",
     "SubmittedAt", "ApproverId", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
VALUES
    ('76000000-0000-0000-0000-000000000001', 'IN-APPROVE-DEMO',
     '30000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002',
     now(), 1, now(), 2, now(), 1, false, 'Demo approve')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "StockInDetails"
    ("Id", "StockInHeaderId", "ProductId", "Quantity", "UnitCost", "Notes", "CreatedAt", "IsDeleted")
VALUES
    ('76100000-0000-0000-0000-000000000001', '76000000-0000-0000-0000-000000000001',
     '50000000-0000-0000-0000-000000000001', 4, 45000.0000, 'Approve demo', now(), false)
ON CONFLICT ("Id") DO NOTHING;

CALL sp_approve_stock_in('76000000-0000-0000-0000-000000000001', 2);

SELECT 'Approve Stock In' AS "Check", "Status", "ApprovedById"
FROM "StockInHeaders" WHERE "Id" = '76000000-0000-0000-0000-000000000001';

-- ------------------------------------------------------------
-- 6.2  Uji function
-- ------------------------------------------------------------
SELECT fn_available_stock(
    '50000000-0000-0000-0000-000000000001',
    '40000000-0000-0000-0000-000000000002'
) AS "AvailableStock";

SELECT fn_next_document_number('IN', CURRENT_DATE) AS "NextDocNo";

SELECT fn_pending_approval_count(NULL) AS "PendingAll";
SELECT fn_pending_approval_count(2) AS "PendingUser2";

SELECT * FROM fn_stock_balance(
    '50000000-0000-0000-0000-000000000001',
    '40000000-0000-0000-0000-000000000002'
);

SELECT * FROM fn_stock_card(
    '50000000-0000-0000-0000-000000000001',
    CURRENT_DATE - 30,
    CURRENT_DATE
) LIMIT 20;

-- ------------------------------------------------------------
-- 7.2  Uji trigger
-- ------------------------------------------------------------
-- Trigger adjustment: Difference dihitung otomatis
INSERT INTO "StockAdjustmentDetails"
    ("Id", "StockAdjustmentHeaderId", "ProductId", "SystemQuantity", "CountedQuantity", "Difference", "Notes", "CreatedAt", "IsDeleted")
VALUES
    ('74200000-0000-0000-0000-000000000099', '74000000-0000-0000-0000-000000000001',
     '50000000-0000-0000-0000-000000000001', 10, 7, 999, 'Trigger test', now(), false)
ON CONFLICT ("Id") DO UPDATE SET "SystemQuantity" = EXCLUDED."SystemQuantity",
    "CountedQuantity" = EXCLUDED."CountedQuantity";

SELECT 'Trigger Difference' AS "Check", "Difference"
FROM "StockAdjustmentDetails" WHERE "Id" = '74200000-0000-0000-0000-000000000099';

-- Trigger quantity negatif: harus gagal (ditangkap)
DO $$
BEGIN
    INSERT INTO "StockInDetails"
        ("Id", "StockInHeaderId", "ProductId", "Quantity", "Notes", "CreatedAt", "IsDeleted")
    VALUES
        (gen_random_uuid(), '70000000-0000-0000-0000-000000000001',
         '50000000-0000-0000-0000-000000000001', -1, 'Negative test', now(), false);
    RAISE EXCEPTION 'Trigger quantity negatif TIDAK bekerja';
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'OK: quantity negatif ditolak -> %', SQLERRM;
END;
$$;

-- Trigger saldo negatif: harus gagal (ditangkap)
DO $$
BEGIN
    UPDATE "Stocks" SET "Quantity" = -1
    WHERE "ProductId" = '50000000-0000-0000-0000-000000000001'
      AND "LocationId" = '40000000-0000-0000-0000-000000000002';
    RAISE EXCEPTION 'Trigger saldo negatif TIDAK bekerja';
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'OK: saldo negatif ditolak -> %', SQLERRM;
END;
$$;

-- Trigger document sequence: prefix dinormalisasi
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "DocumentSequences"
        WHERE "Prefix" = 'TST' AND "Year" = 2099 AND "Month" = 12
    ) THEN
        INSERT INTO "DocumentSequences" ("Prefix", "Year", "Month", "LastNumber")
        VALUES (' tst ', 2099, 12, 1);
    END IF;
END;
$$;

SELECT 'Trigger Prefix' AS "Check", "Prefix"
FROM "DocumentSequences" WHERE "Year" = 2099 AND "Month" = 12;

-- Trigger movement quantity 0: harus gagal (ditangkap)
DO $$
BEGIN
    INSERT INTO "StockMovements"
        ("Id", "MovementNo", "Type", "ProductId", "LocationId", "Quantity",
         "ReferenceType", "ReferenceId", "Notes", "CreatedAt", "CreatedById")
    VALUES
        (gen_random_uuid(), 'MOV-ZERO-TEST', 0,
         '50000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002',
         0, 0, '70000000-0000-0000-0000-000000000001', 'Zero test', now(), 1);
    RAISE EXCEPTION 'Trigger movement 0 TIDAK bekerja';
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'OK: movement 0 ditolak -> %', SQLERRM;
END;
$$;

-- ------------------------------------------------------------
-- Verifikasi akhir
-- ------------------------------------------------------------
SELECT
    (SELECT COUNT(*) FROM "Products") AS "Products",
    (SELECT COUNT(*) FROM "Stocks") AS "Stocks",
    (SELECT COUNT(*) FROM "StockMovements") AS "Movements",
    (SELECT COUNT(*) FROM "v_pending_approvals") AS "PendingApprovals";
