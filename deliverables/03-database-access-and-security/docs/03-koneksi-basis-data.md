# 3. Buatlah Koneksi Basis Data

## a. Pilih teknologi koneksi yang sesuai.

Koneksi memakai Npgsql melalui EF Core. Pengaturan koneksi disimpan di
konfigurasi, dengan nilai yang wajar untuk aplikasi web.

```
Host=db.internal;Port=5432;Database=InventoryAppDB;Username=inventory_app;
Password=<PASSWORD>;SSL Mode=VerifyFull;Pooling=true;Maximum Pool Size=50;Timeout=15
```

| Pengaturan | Arti sederhana |
|---|---|
| `Pooling=true` | Memakai ulang koneksi agar cepat |
| `Maximum Pool Size=50` | Batas koneksi bersamaan |
| `Timeout=15` | Batas waktu membuat koneksi |
| `SSL Mode=VerifyFull` | Koneksi terenkripsi dan terverifikasi |

## b. Tentukan keamanan koneksi.

- Jangan menyimpan password asli di file atau repository.
- Gunakan User Secrets saat pengembangan dan environment variable di server.
- Aktifkan TLS agar data tidak bisa disadap.
- Batasi akses port database hanya dari server aplikasi.
- Jangan memakai akun `postgres` untuk aplikasi.
- Ganti password secara berkala.

## c. Tentukan hak setiap pengguna.

Ada dua lapisan hak akses:

| Lapisan | Contoh | Hak |
|---|---|---|
| Pengguna aplikasi | Admin, Staff, SPV, Manager, Auditor | Menu dan aksi sesuai peran |
| Pengguna database | `inventory_owner`, `inventory_app` | Struktur vs operasi data |

- `inventory_owner` menjalankan migrasi dan perubahan struktur.
- `inventory_app` hanya boleh membaca dan mengubah data, tidak boleh mengubah
  struktur.
- Hak Admin/Staff/SPV/Manager/Auditor diatur aplikasi (RBAC), bukan dengan
  membuat banyak akun database.

```sql
GRANT SELECT, INSERT, UPDATE, DELETE
ON ALL TABLES IN SCHEMA public TO inventory_app;

GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO inventory_app;

REVOKE CREATE ON SCHEMA public FROM PUBLIC;
```
