# 02 - Migrasi dan Seed Data

## 1. Membuat Database

Script: `../sql/01-migration/00-create-database.sql`

```sql
DROP DATABASE IF EXISTS "InventoryAppDB";
CREATE DATABASE "InventoryAppDB" WITH ENCODING 'UTF8' TEMPLATE template0;
```

Jalankan di DBeaver saat terhubung ke database `postgres`.

## 2. Schema dari EF Core Migration

Script: `../sql/01-migration/01-schema.sql`

Dibuat dengan:

```powershell
dotnet ef migrations script `
  --project src/Inventory.Migrations.PostgreSql `
  --startup-project src/Inventory.Web `
  --context AppDbContext `
  --output deliverables/02-database-implementation/sql/01-migration/01-schema.sql `
  --idempotent
```

Isi script:

- Membuat `__EFMigrationsHistory`.
- Membuat seluruh tabel bisnis dan Identity.
- Membuat PK, FK, unique index, filtered index.
- Menulis history migrasi `20261115144223_InitialCreate`.

Karena idempotent, script aman dijalankan ulang; objek yang sudah ada dilewati.

## 3. Seed Data (DML)

Script: `../sql/02-seed-data/02-seed-data.sql`

### Data yang diisi

| Kelompok | Contoh |
|---|---|
| Kategori | General, Consumable |
| Unit | PCS, BOX |
| Supplier | PT Sumber Makmur, CV Cahaya Office |
| Lokasi | HQ (Branch), HQ-WH1, BR2, BR2-WH1 |
| Produk | SKU-001 A4, SKU-002 Toner, SKU-003 Box |
| Stok | HQ-WH1 dan BR2-WH1 |
| Stock In | 2 dokumen approved + detail |
| Stock Out | 1 dokumen approved + detail |
| Transfer | 1 dokumen approved + detail |
| Adjustment | 1 dokumen approved + detail |
| Opname | 1 dokumen approved + detail |
| Movement | 10 baris ledger |
| Document sequence | IN/OUT/TRF/ADJ/OPN |
| Audit | 1 contoh log produk |

### Prinsip DML

- Urutan insert mengikuti FK (master → lokasi → produk → stok → transaksi →
  movement).
- ID memakai UUID tetap agar mudah diverifikasi.
- `ON CONFLICT ("Id") DO NOTHING` membuat seed idempotent.
- `StockMovements` konsisten dengan perubahan saldo.
- Data user/role tidak di-seed karena password Identity dibuat aplikasi.

## 4. Verifikasi Setelah Migrasi

```sql
SELECT COUNT(*) FROM "__EFMigrationsHistory";
SELECT COUNT(*) FROM "Products";
SELECT COUNT(*) FROM "Stocks";
SELECT COUNT(*) FROM "StockMovements";
SELECT * FROM "v_stock_balance" ORDER BY "Sku";
```

Hasil yang diharapkan:

- 1 baris history migrasi.
- 3 produk, 4 baris stok, 10 movement.
- Saldo HQ: SKU-001 = 70, SKU-002 = 18.
- Saldo BR2: SKU-001 = 58, SKU-003 = 30.

## 5. Catatan

- Jika aplikasi dijalankan setelah script ini, `DatabaseBootstrapper` akan
  melihat migrasi sudah diterapkan dan tidak membuat ulang schema.
- Seeding aplikasi hanya mengisi bila `Categories` kosong; karena sudah ada,
  seed aplikasi tidak menimpa data contoh.
