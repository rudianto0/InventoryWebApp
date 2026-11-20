# 2. Menggunakan Fitur Aplikasi SQL untuk Membuat Basis Data

## 2.1 Identifikasi fitur pengolahan DDL

DDL adalah perintah untuk membuat dan mengubah struktur database, seperti
membuat database, tabel, kolom, kunci relasi, dan indeks.

| Fitur | Kegunaan |
|---|---|
| `CREATE DATABASE` | Membuat database |
| `CREATE TABLE` | Membuat tabel |
| `ALTER TABLE` | Menambah kolom atau aturan |
| `CREATE INDEX` | Mempercepat pencarian |
| `DROP ... IF EXISTS` | Menghapus objek lama dengan aman |

**Gambaran PDM (Physical Data Model)**

| Kelompok | Tabel |
|---|---|
| Master | `Categories`, `Units`, `Products`, `Suppliers`, `Locations` |
| Persediaan | `Stocks`, `StockMovements` |
| Transaksi | header/detail Stock In, Stock Out, Transfer, Adjustment, Opname |
| Sistem | `DocumentSequences`, `AuditLogs`, `Users`, `Roles`, `UserRoles` |

Tambahan field sesuai kebutuhan aplikasi: `Id` bertipe UUID, `Version` pada
`Stocks` untuk mencegah data tertimpa, `IsDeleted` untuk soft delete,
`DocumentSequences` untuk nomor dokumen, dan `AuditLogs` untuk riwayat.

## 2.2 Eksekusi fitur pengolahan DDL sesuai kebutuhan

Script: `sql/01-migration/00-create-database.sql` dan `01-schema.sql`.

```sql
CREATE DATABASE "InventoryAppDB" WITH ENCODING 'UTF8' TEMPLATE template0;

CREATE TABLE "Products" (
    "Id" uuid PRIMARY KEY,
    "Sku" varchar(50) NOT NULL,
    "Name" varchar(200) NOT NULL,
    "ReorderLevel" integer NOT NULL,
    "IsDeleted" boolean NOT NULL DEFAULT false
);
```

Hasil: sekitar 27 tabel (20 tabel bisnis dan 7 tabel pengguna), lengkap dengan
kunci utama, kunci relasi, dan aturan unik.
