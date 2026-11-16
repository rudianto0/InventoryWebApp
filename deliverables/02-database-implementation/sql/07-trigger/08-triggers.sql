-- ============================================================
-- Inventory App - Triggers (tanpa audit trigger)
-- PostgreSQL / InventoryAppDB
-- ============================================================

-- Validasi quantity detail transaksi > 0
CREATE OR REPLACE FUNCTION fn_validate_positive_quantity()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW."Quantity" IS NULL OR NEW."Quantity" <= 0 THEN
        RAISE EXCEPTION 'Quantity harus lebih besar dari 0';
    END IF;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_stockin_details_qty ON "StockInDetails";
CREATE TRIGGER trg_stockin_details_qty
BEFORE INSERT OR UPDATE OF "Quantity" ON "StockInDetails"
FOR EACH ROW EXECUTE FUNCTION fn_validate_positive_quantity();

DROP TRIGGER IF EXISTS trg_stockout_details_qty ON "StockOutDetails";
CREATE TRIGGER trg_stockout_details_qty
BEFORE INSERT OR UPDATE OF "Quantity" ON "StockOutDetails"
FOR EACH ROW EXECUTE FUNCTION fn_validate_positive_quantity();

DROP TRIGGER IF EXISTS trg_transfer_details_qty ON "StockTransferDetails";
CREATE TRIGGER trg_transfer_details_qty
BEFORE INSERT OR UPDATE OF "Quantity" ON "StockTransferDetails"
FOR EACH ROW EXECUTE FUNCTION fn_validate_positive_quantity();

-- Stocks tidak boleh negatif
CREATE OR REPLACE FUNCTION fn_stocks_nonnegative()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW."Quantity" < 0 THEN
        RAISE EXCEPTION 'Saldo stok tidak boleh negatif (ProductId=%, LocationId=%)',
            NEW."ProductId", NEW."LocationId";
    END IF;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_stocks_nonnegative ON "Stocks";
CREATE TRIGGER trg_stocks_nonnegative
BEFORE INSERT OR UPDATE OF "Quantity" ON "Stocks"
FOR EACH ROW EXECUTE FUNCTION fn_stocks_nonnegative();

-- Adjustment: hitung Difference otomatis dan validasi counted >= 0
CREATE OR REPLACE FUNCTION fn_adjustment_calculate()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW."CountedQuantity" IS NULL OR NEW."CountedQuantity" < 0 THEN
        RAISE EXCEPTION 'CountedQuantity tidak boleh negatif';
    END IF;
    NEW."Difference" := NEW."CountedQuantity" - NEW."SystemQuantity";
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_adjustment_calculate ON "StockAdjustmentDetails";
CREATE TRIGGER trg_adjustment_calculate
BEFORE INSERT OR UPDATE ON "StockAdjustmentDetails"
FOR EACH ROW EXECUTE FUNCTION fn_adjustment_calculate();

-- Opname: hitung Difference jika CountedQuantity diisi
CREATE OR REPLACE FUNCTION fn_opname_calculate()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW."CountedQuantity" IS NOT NULL THEN
        IF NEW."CountedQuantity" < 0 THEN
            RAISE EXCEPTION 'CountedQuantity tidak boleh negatif';
        END IF;
        NEW."Difference" := NEW."CountedQuantity" - NEW."SystemQuantity";
    ELSE
        NEW."Difference" := 0;
    END IF;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_opname_calculate ON "StockOpnameDetails";
CREATE TRIGGER trg_opname_calculate
BEFORE INSERT OR UPDATE ON "StockOpnameDetails"
FOR EACH ROW EXECUTE FUNCTION fn_opname_calculate();

-- DocumentSequences: normalisasi prefix + validasi nomor
CREATE OR REPLACE FUNCTION fn_document_sequence_guard()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW."Prefix" := upper(trim(NEW."Prefix"));
    IF NEW."LastNumber" < 0 THEN
        RAISE EXCEPTION 'LastNumber tidak boleh negatif';
    END IF;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_document_sequence_guard ON "DocumentSequences";
CREATE TRIGGER trg_document_sequence_guard
BEFORE INSERT OR UPDATE ON "DocumentSequences"
FOR EACH ROW EXECUTE FUNCTION fn_document_sequence_guard();

-- Stock movement: quantity tidak boleh 0
CREATE OR REPLACE FUNCTION fn_movement_nonzero()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW."Quantity" = 0 THEN
        RAISE EXCEPTION 'Quantity movement tidak boleh 0';
    END IF;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_movement_nonzero ON "StockMovements";
CREATE TRIGGER trg_movement_nonzero
BEFORE INSERT OR UPDATE OF "Quantity" ON "StockMovements"
FOR EACH ROW EXECUTE FUNCTION fn_movement_nonzero();
