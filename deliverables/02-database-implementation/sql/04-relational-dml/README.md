# 02.04 - Operasi Relasional (DML)

## Konteks

Contoh DML antar tabel dan view: JOIN, subquery, CTE, `INSERT ... SELECT`,
`UPDATE ... FROM`, `DELETE ... USING`, serta DML melalui view sederhana.

## File

- `05-relational-dml.sql`

## Prasyarat

Schema, seed, index, dan view sudah dibuat.

## Verifikasi

```sql
SELECT * FROM "v_reorder_alert";
SELECT * FROM "v_pending_approvals";
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM "v_stock_balance" WHERE "Quantity" <= "ReorderLevel";
```

## Catatan

View kompleks bersifat read-only; DML hanya pada `v_product_basic`.
