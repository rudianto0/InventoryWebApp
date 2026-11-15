# Operasi dan Akses Basis Data

Dokumen ini menjelaskan bagaimana aplikasi Inventory mengoperasikan database
PostgreSQL: menyimpan/mengubah data, membaca informasi, memakai indeks,
mengakses data dari aplikasi, mengelola koneksi dan hak akses, serta menguji
logika dan performa akses data.

## Ruang Lingkup

- Database: PostgreSQL (`InventoryAppDB`).
- Aplikasi: ASP.NET Core MVC + EF Core 10 + Npgsql.
- Tool manual: DBeaver.
- Dokumen terkait: `docs/database/` (schema, seed, view, procedure, function,
  trigger, transaksi).

## Daftar Dokumen

| File | Isi |
|---|---|
| `00-ringkasan.md` | Ringkasan implementasi dan temuan |
| `01-tujuan-dan-lingkup.md` | Tujuan, aktor, batasan |
| `02-teknologi-akses-data.md` | EF Core, Npgsql, pola akses |
| `03-objek-database-yang-digunakan.md` | Tabel, view, index, function, procedure |
| `04-operasi-basis-data.md` | Simpan/ubah, query, indeks |
| `05-prosedur-akses-basis-data.md` | Library dan perintah akses data |
| `06-koneksi-basis-data.md` | Teknologi koneksi, keamanan, hak akses |
| `07-pengujian-program-basis-data.md` | Skenario uji, logika, performa |
| `Inventory-Database-Access.docx` | Gabungan seluruh dokumen dalam Word |

## Script SQL Pendukung

| File | Isi |
|---|---|
| `sql/postgresql/10-roles-and-grants.sql` | Role `inventory_owner` + `inventory_app`, grant/revoke |
| `sql/postgresql/11-performance-tests.sql` | EXPLAIN ANALYZE, statistik query, uji index |

## Catatan Keamanan

- Connection string di dokumen memakai placeholder, bukan password asli.
- Password harus disimpan di User Secrets, environment variable, atau vault.
- Aplikasi tidak memakai user `postgres` di produksi.
