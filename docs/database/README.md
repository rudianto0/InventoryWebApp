# Database Implementation - PostgreSQL

Dokumen dan script SQL untuk membuat database **InventoryAppDB** di PostgreSQL,
mengisi data, membuat indeks/view, menjalankan operasi relasional, serta
membuat stored procedure, function, trigger, dan simulasi commit/rollback.

## Target

- Database: **PostgreSQL**
- Nama database: **InventoryAppDB**
- Tool eksekusi: **DBeaver** (SQL Editor → Execute SQL Script)
- Schema dibuat dari **EF Core migration script** aplikasi.

## Daftar File

| File | Isi |
|---|---|
| `Inventory-Database-Implementation.docx` | Dokumen lengkap format Word |
| `00-ringkasan-implementasi.md` | Ringkasan implementasi |
| `01-lingkup-dan-arsitektur-database.md` | Lingkup dan arsitektur DB |
| `02-migrasi-dan-seed-data.md` | Migrasi dan pengisian data |
| `03-indeks-dan-view.md` | Indeks dan view |
| `04-operasi-relasional.md` | DML antar tabel dan view |
| `05-stored-procedure.md` | Stored procedure |
| `06-function.md` | Function |
| `07-trigger.md` | Trigger (tanpa audit trigger) |
| `08-transaksi-commit-rollback.md` | Commit, rollback, savepoint |

## Script SQL

```
sql/postgresql/
├─ 00-create-database.sql
├─ 01-schema.sql
├─ 02-seed-data.sql
├─ 03-indexes.sql
├─ 04-views.sql
├─ 05-relational-dml.sql
├─ 06-stored-procedures.sql
├─ 07-functions.sql
├─ 08-triggers.sql
├─ 09-transactions.sql
├─ 10-roles-and-grants.sql      # lihat docs/database-access
├─ 11-performance-tests.sql     # lihat docs/database-access
└─ 99-run-all.sql
```

## Urutan Eksekusi di DBeaver

1. Koneksi ke database **postgres** → jalankan `00-create-database.sql`.
2. Ganti koneksi ke **InventoryAppDB**.
3. Jalankan berurutan:
   `01-schema.sql` → `02-seed-data.sql` → `03-indexes.sql` → `04-views.sql`
   → `05-relational-dml.sql` → `06-stored-procedures.sql` → `07-functions.sql`
   → `08-triggers.sql` → `09-transactions.sql`.
4. Alternatif: setelah database dibuat, jalankan `99-run-all.sql` pada database
   **InventoryAppDB**.

> Semua script kompatibel dengan DBeaver dan tidak memakai meta-command `psql`
> seperti `\i` atau `\echo`.

## Catatan

- Agar aplikasi memakai database ini, ubah `ConnectionStrings:Default` di
  `src/Inventory.Web/appsettings.json` menjadi `Database=InventoryAppDB`.
- Koneksi string/password tidak disimpan di dokumen ini.
- Schema mengikuti aplikasi: quantity `integer`, `UnitCost numeric(18,4)`,
  enum disimpan sebagai `integer`, nama tabel/kolom PascalCase.
- Tidak ada trigger audit; audit tetap ditangani aplikasi.
