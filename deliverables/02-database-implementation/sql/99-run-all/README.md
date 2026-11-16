# 02.99 - Run All

## Konteks

Script gabungan untuk menjalankan seluruh tahap implementasi database.

## File

- `99-run-all.sql`

## Cakupan

`01-schema` → `02-seed` → `03-indexes` → `04-views` → `05-relational-dml`
→ `06-procedures` → `07-functions` → `08-triggers` → `09-transactions`.

## Prasyarat

Database `InventoryAppDB` sudah dibuat dengan
`sql/01-migration/00-create-database.sql`.

## Catatan

- Tidak memakai meta-command `psql`, kompatibel DBeaver.
- Untuk eksekusi bertahap, gunakan folder `01`–`08`.
