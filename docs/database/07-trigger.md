# 07 - Trigger

Script: `sql/postgresql/08-triggers.sql`

Sesuai keputusan, **tidak ada trigger audit**. Audit tetap dilakukan aplikasi
melalui `AppDbContext`.

## Daftar Trigger

| Trigger | Tabel | Aksi |
|---|---|---|
| `trg_stockin_details_qty` | `StockInDetails` | Tolak quantity ≤ 0 |
| `trg_stockout_details_qty` | `StockOutDetails` | Tolak quantity ≤ 0 |
| `trg_transfer_details_qty` | `StockTransferDetails` | Tolak quantity ≤ 0 |
| `trg_stocks_nonnegative` | `Stocks` | Tolak saldo negatif |
| `trg_adjustment_calculate` | `StockAdjustmentDetails` | Hitung `Difference` |
| `trg_opname_calculate` | `StockOpnameDetails` | Hitung `Difference` |
| `trg_document_sequence_guard` | `DocumentSequences` | Normalisasi prefix + validasi |
| `trg_movement_nonzero` | `StockMovements` | Tolak quantity 0 |

## Contoh

### Validasi quantity

```sql
INSERT INTO "StockInDetails" (..., "Quantity")
VALUES (..., 0);  -- gagal
```

### Saldo negatif

```sql
UPDATE "Stocks" SET "Quantity" = -1 WHERE ...;  -- gagal
```

### Difference otomatis

```sql
INSERT INTO "StockAdjustmentDetails"
    ("SystemQuantity", "CountedQuantity", "Difference", ...)
VALUES (10, 7, 999, ...);

SELECT "Difference";  -- menjadi -3
```

### Normalisasi prefix

```sql
INSERT INTO "DocumentSequences" ("Prefix", ...) VALUES (' tst ', ...);
SELECT "Prefix";  -- menjadi TST
```

## Uji Kesesuaian

Script `09-transactions.sql` menguji trigger dengan `DO $$ ... EXCEPTION ... $$`
agar error yang diharapkan tidak menghentikan eksekusi script. Output berupa
`NOTICE` jika trigger bekerja.

## Catatan

- Trigger tidak mengubah `Version` stok; concurrency tetap ditangani aplikasi.
- Trigger kalkulasi `Difference` konsisten dengan logika service aplikasi.
- Trigger validasi bersifat pertahanan terakhir di level database.
