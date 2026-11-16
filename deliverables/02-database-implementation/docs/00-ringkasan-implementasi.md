# 00 - Ringkasan Implementasi Database

## Tujuan

Membangun database operasional **InventoryAppDB** di PostgreSQL yang konsisten
dengan aplikasi Inventory, kemudian melengkapi data, indeks, view, stored
procedure, function, trigger, dan demonstrasi transaksi.

## Hasil yang Disediakan

1. Schema hasil migrasi EF Core (seluruh tabel, FK, index, dan history migrasi).
2. Data contoh untuk master, stok, dan lima jenis transaksi.
3. Indeks tambahan untuk laporan dan pencarian.
4. View untuk saldo stok, reorder alert, pending approval, mutasi harian,
   kartu stok, audit summary, dan view sederhana yang dapat di-update.
5. Operasi DML relasional (JOIN, CTE, INSERT SELECT, UPDATE FROM, DELETE USING).
6. Stored procedure untuk penerimaan, pengeluaran, transfer, approval, dan
   rekalkulasi stok.
7. Function untuk saldo, nomor dokumen, pending approval, dan kartu stok.
8. Trigger validasi dan kalkulasi (tanpa audit trigger).
9. Demonstrasi COMMIT, ROLLBACK, dan SAVEPOINT.

## Karakteristik Database

| Aspek | Nilai |
|---|---|
| Nama database | `InventoryAppDB` |
| DBMS | PostgreSQL |
| Tabel bisnis | ±20 tabel |
| Tabel Identity | 7 tabel (`Users`, `Roles`, `UserRoles`, dll.) |
| Tipe quantity | `integer` |
| Tipe biaya | `numeric(18,4)` |
| ID bisnis | `uuid` (time-ordered dari aplikasi) |
| ID user | `integer` |
| Soft delete | `IsDeleted` + query filter aplikasi |
| Audit | `AuditLogs` ditulis aplikasi |

## Cara Menjalankan Singkat

1. Buat database dengan `00-create-database.sql` (koneksi ke `postgres`).
2. Jalankan `99-run-all.sql` pada database `InventoryAppDB` di DBeaver.
3. Periksa hasil pada bagian verifikasi di akhir script.

## Risiko dan Catatan

- Jangan menjalankan `01-schema.sql` pada database yang sudah memiliki schema
  berbeda tanpa backup.
- `00-create-database.sql` menghapus database `InventoryAppDB` jika sudah ada.
- Script seed idempotent, tetapi procedure test menambah data contoh.
- Trigger tidak menangani audit agar tidak terjadi duplikasi dengan aplikasi.
