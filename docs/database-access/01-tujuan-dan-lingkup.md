# 01 - Tujuan dan Lingkup

## 1.1 Tujuan

- Menjelaskan cara aplikasi menyimpan dan mengubah data ke PostgreSQL.
- Menjelaskan query yang digunakan untuk menghasilkan informasi.
- Menjelaskan pemanfaatan indeks.
- Menjelaskan library dan perintah akses data yang digunakan.
- Menjelaskan desain koneksi dan hak akses.
- Menyediakan skenario pengujian fungsional dan performa.

## 1.2 Ruang Lingkup

| Area | Termasuk |
|---|---|
| Modul | Auth, user, catalog, supplier, lokasi, stock in/out/transfer, adjustment, opname, report, audit |
| Database | PostgreSQL `InventoryAppDB` |
| Akses aplikasi | EF Core + Npgsql |
| Akses manual | DBeaver (DDL/DML/verifikasi) |
| Keamanan | Connection string, TLS, role database, least privilege |
| Pengujian | Fungsional, keamanan, konkurensi, performa |

Di luar lingkup:

- Tuning level OS/kernel PostgreSQL.
- Replikasi/HA tingkat lanjut.
- Multi-tenant.
- Benchmark vendor hardware.

## 1.3 Aktor

### Aktor Aplikasi (RBAC)

| Aktor | Hak |
|---|---|
| Admin | Kelola user, master data, konfigurasi |
| Staff | Transaksi stok, opname, adjustment |
| SPV | Approve transaksi |
| Manager | Dashboard, laporan, approval |
| Auditor | Read-only laporan dan audit |

### Aktor Database

| Aktor | Fungsi |
|---|---|
| `inventory_owner` | Pemilik schema, menjalankan migrasi/DDL |
| `inventory_app` | Service account aplikasi: DML + execute function/procedure |
| `postgres` | Superuser, hanya untuk administrasi darurat |

Catatan: role aplikasi **tidak** dibuat sebagai user database terpisah. Aplikasi
memakai satu service account; pembatasan per user dilakukan di lapisan aplikasi.

## 1.4 Batasan

- Kredensial tidak boleh ditulis di dokumen/repository.
- Semua nama tabel/kolom PostgreSQL menggunakan PascalCase dan quoted.
- Quantity `integer`; `UnitCost numeric(18,4)`.
- Enum disimpan sebagai `integer`.
- Environment pengujian performa harus terpisah dari produksi.
