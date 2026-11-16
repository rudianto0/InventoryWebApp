-- ============================================================
-- Inventory App - Functions
-- PostgreSQL / InventoryAppDB
-- ============================================================

-- Saldo tersedia untuk produk di lokasi
CREATE OR REPLACE FUNCTION fn_available_stock(
    p_product_id uuid,
    p_location_id uuid
) RETURNS integer
LANGUAGE sql
STABLE
AS $$
    SELECT COALESCE((
        SELECT s."Quantity"
        FROM "Stocks" s
        WHERE s."ProductId" = p_product_id
          AND s."LocationId" = p_location_id
    ), 0);
$$;

-- Nomor dokumen atomic: PREFIX-YYYYMM-NNNN
CREATE OR REPLACE FUNCTION fn_next_document_number(
    p_prefix text,
    p_date date DEFAULT CURRENT_DATE
) RETURNS text
LANGUAGE plpgsql
AS $$
DECLARE
    v_prefix text := upper(trim(p_prefix));
    v_year integer := EXTRACT(YEAR FROM p_date)::int;
    v_month integer := EXTRACT(MONTH FROM p_date)::int;
    v_num integer;
BEGIN
    INSERT INTO "DocumentSequences" ("Prefix", "Year", "Month", "LastNumber")
    VALUES (v_prefix, v_year, v_month, 1)
    ON CONFLICT ("Prefix", "Year", "Month")
    DO UPDATE SET "LastNumber" = "DocumentSequences"."LastNumber" + 1
    RETURNING "LastNumber" INTO v_num;

    RETURN v_prefix || '-' || to_char(p_date, 'YYYYMM') || '-' || lpad(v_num::text, 4, '0');
END;
$$;

-- Jumlah pending approval untuk user tertentu (NULL = semua)
CREATE OR REPLACE FUNCTION fn_pending_approval_count(
    p_user_id integer DEFAULT NULL
) RETURNS integer
LANGUAGE sql
STABLE
AS $$
    SELECT COUNT(*)::int
    FROM "v_pending_approvals" v
    WHERE p_user_id IS NULL OR v."ApproverId" = p_user_id;
$$;

-- Saldo stok per produk/lokasi (TABLE)
CREATE OR REPLACE FUNCTION fn_stock_balance(
    p_product_id uuid,
    p_location_id uuid
) RETURNS TABLE (
    "Sku" character varying,
    "ProductName" character varying,
    "LocationName" character varying,
    "Quantity" integer,
    "ReorderLevel" integer
)
LANGUAGE sql
STABLE
AS $$
    SELECT p."Sku", p."Name", l."Name", COALESCE(s."Quantity", 0), p."ReorderLevel"
    FROM "Products" p
    CROSS JOIN "Locations" l
    LEFT JOIN "Stocks" s ON s."ProductId" = p."Id" AND s."LocationId" = l."Id"
    WHERE p."Id" = p_product_id
      AND l."Id" = p_location_id;
$$;

-- Kartu stok dengan saldo berjalan dalam rentang tanggal
CREATE OR REPLACE FUNCTION fn_stock_card(
    p_product_id uuid,
    p_from date,
    p_to date
) RETURNS TABLE (
    "MovementNo" character varying,
    "Type" integer,
    "Quantity" integer,
    "RunningBalance" integer,
    "LocationName" character varying,
    "Notes" character varying,
    "CreatedAt" timestamp with time zone
)
LANGUAGE sql
STABLE
AS $$
    WITH movement AS (
        SELECT
            m."MovementNo",
            m."Type",
            m."Quantity",
            m."Notes",
            m."CreatedAt",
            l."Name" AS "LocationName",
            SUM(m."Quantity") OVER (
                ORDER BY m."CreatedAt", m."Id"
                ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
            ) AS "RunningBalance"
        FROM "StockMovements" m
        JOIN "Locations" l ON l."Id" = m."LocationId"
        WHERE m."ProductId" = p_product_id
    )
    SELECT "MovementNo", "Type", "Quantity", "RunningBalance", "LocationName", "Notes", "CreatedAt"
    FROM movement
    WHERE "CreatedAt" >= p_from::timestamptz
      AND "CreatedAt" < (p_to + 1)::timestamptz
    ORDER BY "CreatedAt", "MovementNo";
$$;
