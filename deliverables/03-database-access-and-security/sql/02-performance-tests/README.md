# 03.02 - Performance Tests

## Konteks

Query untuk mengukur performa dan penggunaan indeks.

## File

- `11-performance-tests.sql`

## Isi

- Ukuran tabel/index.
- Index usage (`pg_stat_user_indexes`).
- `pg_stat_statements` (jika aktif).
- `EXPLAIN (ANALYZE, BUFFERS)` untuk laporan.
- Statistik koneksi dan lock wait.

## Prasyarat

Database terisi data uji. Untuk `pg_stat_statements`:

```sql
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
```

## Catatan

Tidak menjalankan load test; hanya menyediakan query analisis.
