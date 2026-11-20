# Operasi dan Akses Basis Data

Dokumen ini menjelaskan bagaimana aplikasi Inventory mengoperasikan database:
menyimpan/mengubah data, membaca informasi, memakai indeks, mengakses data dari
aplikasi, mengelola koneksi dan hak akses, serta menguji logika dan performa
akses data.

## Target

- Database: **PostgreSQL** atau **SQL Server** (dipilih via `Database:Provider`).
- Aplikasi: ASP.NET Core MVC + EF Core 10 (Npgsql untuk PostgreSQL, provider
  SQL Server untuk SQL Server).
- Tool manual: DBeaver.
- Dokumen terkait: `../../02-database-implementation/docs/`.

## Daftar Dokumen

| File | Butir |
|---|---|
| `01-operasi-basis-data.md` | 1. Operasi basis data (simpan/ubah, query, indeks) |
| `02-prosedur-akses-basis-data.md` | 2. Library dan perintah akses data |
| `03-koneksi-basis-data.md` | 3. Teknologi koneksi, keamanan, hak akses |
| `04-pengujian-program-basis-data.md` | 4. Skenario uji, logika, performa |

## Script SQL Pendukung

| File | Isi |
|---|---|
| `../sql/01-roles-and-grants/10-roles-and-grants.sql` | Role `inventory_owner` + `inventory_app`, grant/revoke |
| `../sql/02-performance-tests/11-performance-tests.sql` | EXPLAIN ANALYZE, statistik query, uji indeks |

## Catatan Keamanan

- Connection string di dokumen memakai placeholder, bukan password asli.
- Password harus disimpan di User Secrets, environment variable, atau vault.
- Aplikasi tidak memakai user `postgres` di produksi.
