# 02.07 - Trigger

## Konteks

Trigger validasi dan kalkulasi di level database. Tidak ada audit trigger;
audit tetap ditangani aplikasi.

## File

- `08-triggers.sql`

## Trigger

| Trigger | Tabel | Aksi |
|---|---|---|
| `trg_stockin_details_qty` | StockInDetails | Tolak qty ≤ 0 |
| `trg_stockout_details_qty` | StockOutDetails | Tolak qty ≤ 0 |
| `trg_transfer_details_qty` | StockTransferDetails | Tolak qty ≤ 0 |
| `trg_stocks_nonnegative` | Stocks | Tolak saldo negatif |
| `trg_adjustment_calculate` | StockAdjustmentDetails | Hitung Difference |
| `trg_opname_calculate` | StockOpnameDetails | Hitung Difference |
| `trg_document_sequence_guard` | DocumentSequences | Normalisasi prefix |
| `trg_movement_nonzero` | StockMovements | Tolak qty 0 |

## Verifikasi

```sql
SELECT tgname, relname
FROM pg_trigger t
JOIN pg_class c ON c.oid = t.tgrelid
WHERE NOT tgisinternal ORDER BY relname, tgname;
```
