# 03 - Database Access and Security

## Konteks

Dokumentasi cara aplikasi mengakses database: library, perintah, koneksi,
keamanan, hak akses, dan pengujian performa.

## Dokumentasi

| File | Isi |
|---|---|
| `docs/00-ringkasan.md` | Ringkasan & temuan |
| `docs/01-tujuan-dan-lingkup.md` | Tujuan, aktor, batasan |
| `docs/02-teknologi-akses-data.md` | EF Core, Npgsql, pola akses |
| `docs/03-objek-database-yang-digunakan.md` | Tabel, view, function, procedure |
| `docs/04-operasi-basis-data.md` | DML, query, index |
| `docs/05-prosedur-akses-basis-data.md` | Library & perintah akses |
| `docs/06-koneksi-basis-data.md` | Koneksi, TLS, role |
| `docs/07-pengujian-program-basis-data.md` | Skenario & performa |
| `Inventory-Database-Access.docx` | Word |

## SQL

| Folder | File | Konteks |
|---|---|---|
| `sql/01-roles-and-grants` | `10-roles-and-grants.sql` | Role `inventory_owner` + `inventory_app` |
| `sql/02-performance-tests` | `11-performance-tests.sql` | EXPLAIN & statistik |

## Sumber

Commit `169a891` (dokumentasi + SQL dipindahkan ke section ini).

## Catatan

Password pada dokumen memakai placeholder. Jangan commit kredensial asli.
