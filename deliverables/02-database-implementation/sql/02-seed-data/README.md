# 02.02 - Seed Data (DML)

## Konteks

Mengisi data contoh: kategori, unit, supplier, lokasi, produk, stok, lima tipe
transaksi, movement, document sequence, dan audit.

## File

- `02-seed-data.sql`

## Prasyarat

`sql/01-migration/01-schema.sql` sudah dijalankan.

## Sifat

- Idempotent (`ON CONFLICT DO NOTHING`).
- Urutan insert mengikuti FK.
- Menggunakan UUID tetap agar mudah diverifikasi.

## Verifikasi

```sql
SELECT COUNT(*) FROM "Products";
SELECT COUNT(*) FROM "Stocks";
SELECT COUNT(*) FROM "StockMovements";
SELECT * FROM "v_stock_balance" ORDER BY "Sku";
```
