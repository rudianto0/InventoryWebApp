# Inventory Deliverables

Kumpulan hasil dokumentasi dan SQL yang dikelompokkan per section/konteks.
Folder ini adalah **salinan terorganisir** dari commit:

- `1d5f316` — scalability analysis + PostgreSQL database implementation
- `169a891` — code documentation, database access, programming fundamentals

## Struktur

| Section | Isi | Sumber |
|---|---|---|
| `01-scalability-analysis` | Analisis skalabilitas + Word | `1d5f316` |
| `02-database-implementation` | Migrasi, seed, index, view, DML, procedure, function, trigger, transaksi + Word | `1d5f316` |
| `03-database-access-and-security` | Library akses, koneksi, keamanan, role, performa + Word | `169a891` |
| `04-programming-fundamentals` | Tipe data, flowchart, algoritma, prosedur/fungsi, kompleksitas + Word | `169a891` |
| `05-code-documentation` | Dokumentasi modul/method/eksepsi, annotated code, DocFX | `169a891` |
| `06-code-preparation-and-debugging` | Persiapan kode, debugging tools, build, log kesalahan, perbaikan | working tree |
| `07-performance-profiling` | Pengukuran waktu/memori, bottleneck, solusi, kompleksitas | working tree |
| `08-code-guidelines-compliance` | Kesesuaian kode, perbaikan, pengecualian | working tree |
| `09-software-testing` | Kebutuhan, data, skenario, hasil, coverage | working tree |
| `99-source-map` | Peta file, commit, dan manifest | keduanya |

## Urutan Membaca

1. `01-scalability-analysis/README.md`
2. `02-database-implementation/README.md`
3. `03-database-access-and-security/README.md`
4. `04-programming-fundamentals/README.md`
5. `05-code-documentation/README.md`
6. `06-code-preparation-and-debugging/README.md`
7. `07-performance-profiling/README.md`
8. `08-code-guidelines-compliance/README.md`
9. `09-software-testing/README.md`

## SQL

SQL tidak lagi dikumpulkan dalam satu folder. Setiap konteks memiliki folder:

```
02-database-implementation/sql/
├─ 01-migration
├─ 02-seed-data
├─ 03-index-and-view
├─ 04-relational-dml
├─ 05-stored-procedure
├─ 06-function
├─ 07-trigger
├─ 08-transaction
└─ 99-run-all
```

SQL akses/keamanan berada di:

```
03-database-access-and-security/sql/
├─ 01-roles-and-grants
└─ 02-performance-tests
```

## Catatan

- File asli di `docs/` dan `sql/` sudah dihapus; `deliverables/` adalah satu-satunya salinan.
- `appsettings.json` tidak disalin (mengandung kredensial).
- `api-site/` tidak disalin; dapat digenerate ulang.
- Semua SQL target PostgreSQL, dijalankan melalui DBeaver.
- Detail file ada di `MANIFEST.md` dan `99-source-map/file-manifest.csv`.
