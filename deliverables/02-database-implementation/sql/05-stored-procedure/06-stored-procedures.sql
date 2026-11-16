-- ============================================================
-- Inventory App - Stored Procedures
-- PostgreSQL / InventoryAppDB
-- ============================================================

-- ------------------------------------------------------------
-- sp_receive_stock: posting penerimaan stok (langsung approved)
-- ------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_receive_stock(
    INOUT p_doc_id uuid,
    IN p_supplier_id uuid,
    IN p_location_id uuid,
    IN p_product_id uuid,
    IN p_qty integer,
    IN p_user_id integer
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_year integer := EXTRACT(YEAR FROM now())::int;
    v_month integer := EXTRACT(MONTH FROM now())::int;
    v_num integer;
    v_docno text;
BEGIN
    IF p_qty IS NULL OR p_qty <= 0 THEN
        RAISE EXCEPTION 'Quantity harus lebih besar dari 0';
    END IF;

    IF p_doc_id IS NULL THEN
        p_doc_id := gen_random_uuid();
    END IF;

    INSERT INTO "DocumentSequences" ("Prefix", "Year", "Month", "LastNumber")
    VALUES ('IN', v_year, v_month, 1)
    ON CONFLICT ("Prefix", "Year", "Month")
    DO UPDATE SET "LastNumber" = "DocumentSequences"."LastNumber" + 1
    RETURNING "LastNumber" INTO v_num;

    v_docno := 'IN-' || to_char(now(), 'YYYYMM') || '-' || lpad(v_num::text, 4, '0');

    INSERT INTO "StockInHeaders"
        ("Id", "DocNo", "SupplierId", "LocationId", "Date", "Status",
         "SubmittedAt", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
    VALUES
        (p_doc_id, v_docno, p_supplier_id, p_location_id, now(), 2,
         now(), now(), p_user_id, now(), p_user_id, false, 'Dibuat oleh sp_receive_stock');

    INSERT INTO "StockInDetails"
        ("Id", "StockInHeaderId", "ProductId", "Quantity", "UnitCost", "Notes", "CreatedAt", "IsDeleted")
    VALUES
        (gen_random_uuid(), p_doc_id, p_product_id, p_qty, NULL, 'Procedure receive', now(), false);

    INSERT INTO "Stocks"
        ("Id", "ProductId", "LocationId", "Quantity", "UpdatedAt", "Version", "CreatedAt")
    VALUES
        (gen_random_uuid(), p_product_id, p_location_id, p_qty, now(), 1, now())
    ON CONFLICT ("ProductId", "LocationId")
    DO UPDATE SET
        "Quantity" = "Stocks"."Quantity" + EXCLUDED."Quantity",
        "UpdatedAt" = now(),
        "Version" = "Stocks"."Version" + 1;

    INSERT INTO "StockMovements"
        ("Id", "MovementNo", "Type", "ProductId", "LocationId", "Quantity",
         "ReferenceType", "ReferenceId", "Notes", "CreatedAt", "CreatedById")
    VALUES
        (gen_random_uuid(), 'MOV-' || to_char(now(), 'YYYYMM') || '-' || lpad(v_num::text, 4, '0'),
         0, p_product_id, p_location_id, p_qty, 0, p_doc_id,
         'Receive via procedure', now(), p_user_id);
END;
$$;

-- ------------------------------------------------------------
-- sp_issue_stock: pengeluaran stok dengan validasi saldo
-- ------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_issue_stock(
    INOUT p_doc_id uuid,
    IN p_location_id uuid,
    IN p_product_id uuid,
    IN p_qty integer,
    IN p_user_id integer
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_year integer := EXTRACT(YEAR FROM now())::int;
    v_month integer := EXTRACT(MONTH FROM now())::int;
    v_num integer;
    v_docno text;
    v_after integer;
BEGIN
    IF p_qty IS NULL OR p_qty <= 0 THEN
        RAISE EXCEPTION 'Quantity harus lebih besar dari 0';
    END IF;

    IF p_doc_id IS NULL THEN
        p_doc_id := gen_random_uuid();
    END IF;

    INSERT INTO "DocumentSequences" ("Prefix", "Year", "Month", "LastNumber")
    VALUES ('OUT', v_year, v_month, 1)
    ON CONFLICT ("Prefix", "Year", "Month")
    DO UPDATE SET "LastNumber" = "DocumentSequences"."LastNumber" + 1
    RETURNING "LastNumber" INTO v_num;

    v_docno := 'OUT-' || to_char(now(), 'YYYYMM') || '-' || lpad(v_num::text, 4, '0');

    UPDATE "Stocks"
    SET "Quantity" = "Quantity" - p_qty,
        "UpdatedAt" = now(),
        "Version" = "Version" + 1
    WHERE "ProductId" = p_product_id
      AND "LocationId" = p_location_id
      AND "Quantity" >= p_qty
    RETURNING "Quantity" INTO v_after;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Stok tidak mencukupi untuk produk % di lokasi %', p_product_id, p_location_id;
    END IF;

    INSERT INTO "StockOutHeaders"
        ("Id", "DocNo", "LocationId", "Date", "Purpose", "Status",
         "SubmittedAt", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
    VALUES
        (p_doc_id, v_docno, p_location_id, now(), 'Procedure issue', 2,
         now(), now(), p_user_id, now(), p_user_id, false, 'Dibuat oleh sp_issue_stock');

    INSERT INTO "StockOutDetails"
        ("Id", "StockOutHeaderId", "ProductId", "Quantity", "Notes", "CreatedAt", "IsDeleted")
    VALUES
        (gen_random_uuid(), p_doc_id, p_product_id, p_qty, 'Procedure issue', now(), false);

    INSERT INTO "StockMovements"
        ("Id", "MovementNo", "Type", "ProductId", "LocationId", "Quantity",
         "ReferenceType", "ReferenceId", "Notes", "CreatedAt", "CreatedById")
    VALUES
        (gen_random_uuid(), 'MOV-' || to_char(now(), 'YYYYMM') || '-' || lpad(v_num::text, 4, '0'),
         1, p_product_id, p_location_id, -p_qty, 1, p_doc_id,
         'Issue via procedure', now(), p_user_id);
END;
$$;

-- ------------------------------------------------------------
-- sp_transfer_stock: transfer antar lokasi
-- ------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_transfer_stock(
    INOUT p_doc_id uuid,
    IN p_from_location_id uuid,
    IN p_to_location_id uuid,
    IN p_product_id uuid,
    IN p_qty integer,
    IN p_user_id integer
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_year integer := EXTRACT(YEAR FROM now())::int;
    v_month integer := EXTRACT(MONTH FROM now())::int;
    v_num integer;
    v_docno text;
BEGIN
    IF p_qty IS NULL OR p_qty <= 0 THEN
        RAISE EXCEPTION 'Quantity harus lebih besar dari 0';
    END IF;
    IF p_from_location_id = p_to_location_id THEN
        RAISE EXCEPTION 'Lokasi asal dan tujuan tidak boleh sama';
    END IF;

    IF p_doc_id IS NULL THEN
        p_doc_id := gen_random_uuid();
    END IF;

    INSERT INTO "DocumentSequences" ("Prefix", "Year", "Month", "LastNumber")
    VALUES ('TRF', v_year, v_month, 1)
    ON CONFLICT ("Prefix", "Year", "Month")
    DO UPDATE SET "LastNumber" = "DocumentSequences"."LastNumber" + 1
    RETURNING "LastNumber" INTO v_num;

    v_docno := 'TRF-' || to_char(now(), 'YYYYMM') || '-' || lpad(v_num::text, 4, '0');

    UPDATE "Stocks"
    SET "Quantity" = "Quantity" - p_qty,
        "UpdatedAt" = now(),
        "Version" = "Version" + 1
    WHERE "ProductId" = p_product_id
      AND "LocationId" = p_from_location_id
      AND "Quantity" >= p_qty;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Stok asal tidak mencukupi';
    END IF;

    INSERT INTO "Stocks"
        ("Id", "ProductId", "LocationId", "Quantity", "UpdatedAt", "Version", "CreatedAt")
    VALUES
        (gen_random_uuid(), p_product_id, p_to_location_id, p_qty, now(), 1, now())
    ON CONFLICT ("ProductId", "LocationId")
    DO UPDATE SET
        "Quantity" = "Stocks"."Quantity" + EXCLUDED."Quantity",
        "UpdatedAt" = now(),
        "Version" = "Stocks"."Version" + 1;

    INSERT INTO "StockTransferHeaders"
        ("Id", "DocNo", "FromLocationId", "ToLocationId", "Date", "Status",
         "SubmittedAt", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "IsDeleted", "Notes")
    VALUES
        (p_doc_id, v_docno, p_from_location_id, p_to_location_id, now(), 2,
         now(), now(), p_user_id, now(), p_user_id, false, 'Dibuat oleh sp_transfer_stock');

    INSERT INTO "StockTransferDetails"
        ("Id", "StockTransferHeaderId", "ProductId", "Quantity", "Notes", "CreatedAt", "IsDeleted")
    VALUES
        (gen_random_uuid(), p_doc_id, p_product_id, p_qty, 'Procedure transfer', now(), false);

    INSERT INTO "StockMovements"
        ("Id", "MovementNo", "Type", "ProductId", "LocationId", "Quantity",
         "ReferenceType", "ReferenceId", "Notes", "CreatedAt", "CreatedById")
    VALUES
        (gen_random_uuid(), 'MOV-' || to_char(now(), 'YYYYMM') || '-' || lpad(v_num::text, 4, '0') || '-OUT',
         2, p_product_id, p_from_location_id, -p_qty, 2, p_doc_id,
         'Transfer out', now(), p_user_id),
        (gen_random_uuid(), 'MOV-' || to_char(now(), 'YYYYMM') || '-' || lpad(v_num::text, 4, '0') || '-IN',
         3, p_product_id, p_to_location_id, p_qty, 2, p_doc_id,
         'Transfer in', now(), p_user_id);
END;
$$;

-- ------------------------------------------------------------
-- sp_approve_stock_in: approve header Submitted + terapkan stok
-- ------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_approve_stock_in(
    IN p_header_id uuid,
    IN p_approver_id integer
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_status integer;
    r record;
BEGIN
    SELECT "Status" INTO v_status
    FROM "StockInHeaders"
    WHERE "Id" = p_header_id
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Dokumen Stock In tidak ditemukan';
    END IF;

    IF v_status <> 1 THEN
        RAISE NOTICE 'Dokumen bukan Submitted (status=%). Tidak ada perubahan.', v_status;
        RETURN;
    END IF;

    FOR r IN
        SELECT "ProductId", "Quantity", "Notes"
        FROM "StockInDetails"
        WHERE "StockInHeaderId" = p_header_id
    LOOP
        INSERT INTO "Stocks"
            ("Id", "ProductId", "LocationId", "Quantity", "UpdatedAt", "Version", "CreatedAt")
        SELECT gen_random_uuid(), r."ProductId", h."LocationId", r."Quantity", now(), 1, now()
        FROM "StockInHeaders" h
        WHERE h."Id" = p_header_id
        ON CONFLICT ("ProductId", "LocationId")
        DO UPDATE SET
            "Quantity" = "Stocks"."Quantity" + EXCLUDED."Quantity",
            "UpdatedAt" = now(),
            "Version" = "Stocks"."Version" + 1;

        INSERT INTO "StockMovements"
            ("Id", "MovementNo", "Type", "ProductId", "LocationId", "Quantity",
             "ReferenceType", "ReferenceId", "Notes", "CreatedAt", "CreatedById")
        SELECT gen_random_uuid(), 'MOV-APR-' || to_char(now(), 'YYYYMMDDHH24MISS') || '-' || left(r."ProductId"::text, 8),
               0, r."ProductId", h."LocationId", r."Quantity", 0, p_header_id,
               COALESCE(r."Notes", 'Approve Stock In'), now(), p_approver_id
        FROM "StockInHeaders" h
        WHERE h."Id" = p_header_id;
    END LOOP;

    UPDATE "StockInHeaders"
    SET "Status" = 2,
        "ApprovedAt" = now(),
        "ApprovedById" = p_approver_id,
        "UpdatedAt" = now(),
        "UpdatedById" = p_approver_id
    WHERE "Id" = p_header_id;
END;
$$;

-- ------------------------------------------------------------
-- sp_recalculate_stock: hitung ulang saldo satu lokasi dari movement
-- ------------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_recalculate_stock(
    IN p_location_id uuid,
    INOUT p_rows integer
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO "Stocks"
        ("Id", "ProductId", "LocationId", "Quantity", "UpdatedAt", "Version", "CreatedAt")
    SELECT
        gen_random_uuid(),
        m."ProductId",
        p_location_id,
        SUM(m."Quantity"),
        now(),
        1,
        now()
    FROM "StockMovements" m
    WHERE m."LocationId" = p_location_id
    GROUP BY m."ProductId"
    ON CONFLICT ("ProductId", "LocationId")
    DO UPDATE SET
        "Quantity" = EXCLUDED."Quantity",
        "UpdatedAt" = now(),
        "Version" = "Stocks"."Version" + 1;

    GET DIAGNOSTICS p_rows = ROW_COUNT;
END;
$$;
