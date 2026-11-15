# 00 - Ringkasan

## Kondisi Saat Ini

- Aplikasi mengakses PostgreSQL melalui **EF Core 10 + Npgsql**.
- Service Application memakai `IAppDbContext` dan `DbSet` langsung (tanpa
  repository layer formal).
- Operasi tulis memakai `SaveChangesAsync`; audit dan soft delete diproses di
  `AppDbContext`.
- Transaksi stok memakai `BeginTransactionAsync` pada approval/ledger.
- Query baca memakai LINQ async, `AsNoTracking`, dan pagination.
- Unit test memakai SQLite in-memory; belum ada integration test PostgreSQL
  atau performance test.

## Temuan Penting

| No | Temuan | Dampak | Rekomendasi |
|---|---|---|---|
| 1 | Connection string berisi password di `appsettings.json` | Risiko kebocoran kredensial | User Secrets/env var/vault |
| 2 | Aplikasi berpotensi memakai `postgres` superuser | Hak terlalu besar | Pisahkan `inventory_owner` dan `inventory_app` |
| 3 | Belum ada retry/timeout eksplisit | Gangguan koneksi sesaat gagal | Konfigurasi Npgsql + retry |
| 4 | ApprovalService N+1 | Lambat saat pending banyak | Batch query/join |
| 5 | Tidak ada integration/performance test | Kapasitas tidak terukur | Uji PostgreSQL + EXPLAIN |
| 6 | Tidak ada QueryTag/log query | Sulit audit performa | Aktifkan logging + tag |

## Hasil yang Disediakan Dokumen

1. Penjelasan operasi DML/query/index.
2. Prosedur akses data dari aplikasi.
3. Desain koneksi aman + role database.
4. Skenario pengujian fungsional, keamanan, dan performa.
5. Script grant dan performance test PostgreSQL.

## Rekomendasi Utama

1. Pindahkan kredensial ke secret store.
2. Gunakan `inventory_app` untuk runtime dan `inventory_owner` untuk migrasi.
3. Aktifkan `SSL Mode=Require`/`VerifyFull` di produksi.
4. Tambah integration test PostgreSQL minimal untuk transaksi stok.
5. Ukur query dengan `EXPLAIN (ANALYZE, BUFFERS)` dan `pg_stat_statements`.
6. Perbaiki N+1 dan tambah index yang belum ada.
