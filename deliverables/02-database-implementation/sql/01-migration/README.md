# 02.01 - Migration

## Konteks

Membuat database `InventoryAppDB` dan schema dari EF Core migration.

## File

| File | Fungsi |
|---|---|
| `00-create-database.sql` | Drop/create database (jalankan di database `postgres`) |
| `01-schema.sql` | Tabel, FK, index, `__EFMigrationsHistory` (idempotent) |

## Urutan

1. Koneksi ke `postgres`, jalankan `00-create-database.sql`.
2. Koneksi ke `InventoryAppDB`, jalankan `01-schema.sql`.

## Verifikasi

```sql
SELECT COUNT(*) FROM "__EFMigrationsHistory";
SELECT table_name FROM information_schema.tables
WHERE table_schema = 'public' ORDER BY table_name;
```

## Catatan

`00-create-database.sql` menghapus database jika sudah ada. Backup dahulu jika perlu.
