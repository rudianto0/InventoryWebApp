# 02.08 - Commit, Rollback, dan Pengujian

## Konteks

Demonstrasi transaksi dan pengujian procedure/function/trigger.

## File

- `09-transactions.sql`

## Isi

1. `BEGIN ... COMMIT` (data tersimpan).
2. `BEGIN ... ROLLBACK` (data dibatalkan).
3. `SAVEPOINT` / `ROLLBACK TO SAVEPOINT`.
4. Uji `sp_*`, `fn_*`, dan trigger.

## Prasyarat

Semua objek database sudah dibuat.

## Verifikasi

```sql
SELECT COUNT(*) FROM "Suppliers" WHERE "Code" = 'SUP-TRX-COMMIT';
SELECT COUNT(*) FROM "Suppliers" WHERE "Code" = 'SUP-TRX-ROLLBACK';
```

## Catatan

Jalankan sebagai script di DBeaver agar `BEGIN`/`COMMIT` satu sesi.
