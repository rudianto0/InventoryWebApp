# Implementasi Database PostgreSQL

Dokumen dan script SQL untuk membangun database **InventoryAppDB** di
PostgreSQL, mengisi data, membuat indeks/view, menjalankan operasi relasional,
serta membuat stored procedure, function, trigger, dan simulasi commit/rollback.

## Target

- Database: **PostgreSQL**
- Nama database: **InventoryAppDB**
- Tool eksekusi: **DBeaver** (SQL Editor)
- Schema berasal dari **EF Core migration** aplikasi.

## Daftar Dokumen

| File | Butir |
|---|---|
| `01-persiapan-software.md` | 1. Persiapan perangkat lunak |
| `02-membuat-basis-data.md` | 2. Membuat basis data (DDL) |
| `03-mengisi-tabel.md` | 3. Mengisi tabel, indeks, view |
| `04-operasi-relasional.md` | 4. Operasi relasional (DML) |
| `05-stored-procedure.md` | 5. Stored procedure |
| `06-function.md` | 6. Function |
| `07-trigger.md` | 7. Trigger |
| `08-commit-rollback.md` | 8. Commit dan rollback |

## Script SQL

```
../sql/
├─ 01-migration/           00-create-database.sql, 01-schema.sql
├─ 02-seed-data/           02-seed-data.sql
├─ 03-index-and-view/      03-indexes.sql, 04-views.sql
├─ 04-relational-dml/      05-relational-dml.sql
├─ 05-stored-procedure/    06-stored-procedures.sql
├─ 06-function/            07-functions.sql
├─ 07-trigger/             08-triggers.sql
├─ 08-transaction/         09-transactions.sql
└─ 99-run-all/             99-run-all.sql
```

## Urutan Eksekusi di DBeaver

1. Koneksi ke database **postgres**, jalankan `00-create-database.sql`.
2. Ganti koneksi ke **InventoryAppDB**.
3. Jalankan berurutan:
   `01-schema.sql` → `02-seed-data.sql` → `03-indexes.sql` → `04-views.sql`
   → `05-relational-dml.sql` → `06-stored-procedures.sql` → `07-functions.sql`
   → `08-triggers.sql` → `09-transactions.sql`.
4. Alternatif: jalankan `99-run-all.sql` pada database **InventoryAppDB**.

## Catatan

- Agar aplikasi memakai database ini, ubah `ConnectionStrings:Default` di
  `src/Inventory.Web/appsettings.json` menjadi `Database=InventoryAppDB`.
- Koneksi string dan password tidak disimpan di dokumen ini.
- Quantity `integer`, `UnitCost numeric(18,4)`, enum disimpan sebagai `integer`.
- Audit ditangani aplikasi; tidak ada trigger audit.
