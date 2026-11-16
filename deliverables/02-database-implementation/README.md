# 02 - Implementasi Database PostgreSQL

## Konteks

Implementasi database `InventoryAppDB` untuk PostgreSQL: migrasi schema,
seed data, indeks, view, operasi relasional, stored procedure, function,
trigger, dan commit/rollback.

## Dokumentasi

- `docs/` — Markdown penjelasan tiap tahap.
- `Inventory-Database-Implementation.docx` — dokumen Word.

## SQL per Konteks

| Folder | File | Prasyarat |
|---|---|---|
| `sql/01-migration` | `00-create-database.sql`, `01-schema.sql` | Koneksi ke database `postgres` |
| `sql/02-seed-data` | `02-seed-data.sql` | Schema sudah dibuat |
| `sql/03-index-and-view` | `03-indexes.sql`, `04-views.sql` | Seed data |
| `sql/04-relational-dml` | `05-relational-dml.sql` | View sudah dibuat |
| `sql/05-stored-procedure` | `06-stored-procedures.sql` | Schema |
| `sql/06-function` | `07-functions.sql` | Schema |
| `sql/07-trigger` | `08-triggers.sql` | Schema |
| `sql/08-transaction` | `09-transactions.sql` | Semua objek di atas |
| `sql/99-run-all` | `99-run-all.sql` | Database sudah dibuat |

## Urutan Eksekusi DBeaver

1. `sql/01-migration/00-create-database.sql` (koneksi ke `postgres`).
2. Ganti koneksi ke `InventoryAppDB`.
3. `sql/01-migration/01-schema.sql`
4. `sql/02-seed-data/02-seed-data.sql`
5. `sql/03-index-and-view/03-indexes.sql`
6. `sql/03-index-and-view/04-views.sql`
7. `sql/04-relational-dml/05-relational-dml.sql`
8. `sql/05-stored-procedure/06-stored-procedures.sql`
9. `sql/06-function/07-functions.sql`
10. `sql/07-trigger/08-triggers.sql`
11. `sql/08-transaction/09-transactions.sql`

Alternatif setelah database dibuat: `sql/99-run-all/99-run-all.sql`.

## Sumber

Commit `1d5f316` (dokumentasi + SQL dipindahkan ke section ini).

## Catatan

- Schema berasal dari EF Core migration script.
- Quantity `integer`, `UnitCost numeric(18,4)`.
- Tidak ada audit trigger; audit ditangani aplikasi.
