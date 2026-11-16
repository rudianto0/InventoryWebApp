# 04 - Operasi Relasional dan DML

Script: `../sql/04-relational-dml/05-relational-dml.sql`

## 4.1 Fitur Pengolahan DML

Contoh yang disediakan:

1. **JOIN multi-tabel** — produk, kategori, unit, stok, lokasi.
2. **Subquery** — produk tanpa baris stok.
3. **CTE + agregasi** — total mutasi per produk.
4. **INSERT ... SELECT** — snapshot reorder alert ke tabel sementara.
5. **UPDATE ... FROM** — sinkronisasi reorder level dari agregasi kategori.
6. **DELETE ... USING** — hapus data demo berdasarkan relasi kategori.
7. **DML melalui view** — insert/update/delete pada `v_product_basic`.
8. **Query efisien** — `EXISTS`, batch insert, `ON CONFLICT`.
9. **EXPLAIN ANALYZE** — melihat rencana eksekusi.

## 4.2 DML Antar Tabel

### JOIN

```sql
SELECT p."Sku", p."Name", c."Name" AS "Category",
       u."Code" AS "Unit", l."Name" AS "Location",
       s."Quantity", p."ReorderLevel"
FROM "Products" p
JOIN "Categories" c ON c."Id" = p."CategoryId"
JOIN "Units" u ON u."Id" = p."UnitId"
LEFT JOIN "Stocks" s ON s."ProductId" = p."Id"
LEFT JOIN "Locations" l ON l."Id" = s."LocationId";
```

### Subquery

```sql
SELECT p."Sku", p."Name"
FROM "Products" p
WHERE NOT EXISTS (
    SELECT 1 FROM "Stocks" s WHERE s."ProductId" = p."Id"
);
```

### CTE

```sql
WITH movement AS (
    SELECT "ProductId", SUM("Quantity") AS "NetQuantity"
    FROM "StockMovements"
    GROUP BY "ProductId"
)
SELECT p."Sku", m."NetQuantity"
FROM movement m JOIN "Products" p ON p."Id" = m."ProductId";
```

### INSERT ... SELECT

```sql
CREATE TEMP TABLE "tmp_reorder_snapshot" AS
SELECT * FROM "v_reorder_alert";
```

### UPDATE ... FROM

```sql
UPDATE "Products" p
SET "ReorderLevel" = x."NewLevel"
FROM (
    SELECT "CategoryId", CEIL(AVG("ReorderLevel"))::int AS "NewLevel"
    FROM "Products" GROUP BY "CategoryId"
) x
WHERE p."CategoryId" = x."CategoryId";
```

### DELETE ... USING

```sql
DELETE FROM "Products" p
USING "Categories" c
WHERE p."CategoryId" = c."Id"
  AND p."Sku" = 'SKU-DEMO-DELETE';
```

## 4.3 DML Antar View

```sql
-- INSERT melalui view sederhana
INSERT INTO "v_product_basic"
    ("Id", "Sku", "Name", "CategoryId", "UnitId",
     "ReorderLevel", "IsActive", "CreatedAt", "IsDeleted")
VALUES (gen_random_uuid(), 'SKU-VIEW-01', 'Produk via View',
        '<category-id>', '<unit-id>', 4, true, now(), false);

-- UPDATE melalui view
UPDATE "v_product_basic"
SET "ReorderLevel" = 6
WHERE "Sku" = 'SKU-VIEW-01';

-- DELETE melalui view
DELETE FROM "v_product_basic" WHERE "Sku" = 'SKU-VIEW-01';
```

View kompleks (`v_pending_approvals`, `v_stock_card`, `v_stock_balance`) hanya
untuk SELECT.

## 4.4 DML Efisien

| Teknik | Alasan |
|---|---|
| `EXISTS` daripada `IN` | Berhenti saat match pertama, aman untuk NULL |
| Batch insert + `ON CONFLICT` | Mengurangi round-trip dan aman diulang |
| Filter tanggal + index | Menghindari full scan |
| `EXPLAIN (ANALYZE, BUFFERS)` | Melihat bottleneck nyata |
| Hindari SELECT * | Mengurangi I/O dan memory |
| Transaksi singkat | Mengurangi lock contention |

## Verifikasi

```sql
SELECT * FROM "v_reorder_alert";
SELECT * FROM "v_pending_approvals";
SELECT * FROM "v_stock_card" ORDER BY "CreatedAt" DESC LIMIT 20;
```
