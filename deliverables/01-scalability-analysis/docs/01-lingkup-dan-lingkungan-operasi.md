# 01 - Lingkup Sistem dan Lingkungan Operasi

## 1.1 Identifikasi Lingkup (Scope) Sistem

### Tujuan Sistem

Menyediakan satu sumber kebenaran (single source of truth) untuk data persediaan
barang, mencatat setiap mutasi stok, dan menyediakan alur approval satu level
serta laporan yang dapat diaudit.

### Aktor

| Aktor | Peran utama |
|---|---|
| Admin | Kelola user, role, master data, konfigurasi |
| Staff | Membuat transaksi stok, opname, adjustment |
| SPV | Approve transaksi dan monitoring stok |
| Manager | Dashboard, laporan, approval tertentu |
| Auditor | Read-only laporan dan audit trail |

### Modul dalam Lingkup

1. Authentication & authorization (Identity + cookie, RBAC).
2. User & role management, relasi atasan (parent).
3. Master produk, kategori, unit.
4. Master lokasi (Branch/Warehouse/Store) dan hierarki.
5. Master supplier.
6. Stock In (receiving) + unit cost.
7. Stock Out (issuing).
8. Stock Transfer antar lokasi.
9. Stock Adjustment dengan alasan.
10. Stock Opname (cycle count).
11. Approval satu level.
12. Dashboard, stock balance, stock card, movement, reorder, CSV export.
13. Audit trail dan soft delete.

### Batas Sistem

- Tidak termasuk integrasi marketplace/e-commerce.
- Tidak termasuk barcode scanner hardware khusus.
- Tidak multi-tenant/multi-company.
- Tidak termasuk payroll/accounting penuh.
- Approval hanya satu level (bukan multi-level enterprise).

### Entitas & Data Utama

- Master: `Categories`, `Units`, `Products`, `Suppliers`, `Locations`.
- Inventory: `Stocks`, `StockMovements`.
- Transaksi: header/detail Stock In, Stock Out, Stock Transfer,
  Stock Adjustment, Stock Opname.
- Sistem: `Users`, `Roles`, `DocumentSequences`, `AuditLogs`.

Karakteristik data:

- ID bisnis `Guid` (time-ordered), ID user `int`.
- Quantity berupa `int`; `UnitCost` `decimal(18,4)`.
- Soft delete pada master dan transaksi.
- Audit create/update/delete otomatis.
- Concurrency token pada `Stocks.Version`.

### Pola Penggunaan

- Mayoritas read: dashboard, daftar transaksi, laporan, pencarian master.
- Write: pembuatan dokumen, submit, approve, dan mutasi stok.
- Puncak beban: awal/akhir bulan, opname periodik, dan export laporan.

## 1.2 Identifikasi Lingkungan Operasi Aplikasi

### Runtime dan Teknologi

| Komponen | Kondisi saat ini |
|---|---|
| Framework | ASP.NET Core MVC .NET 10 |
| UI | Razor Views + Bootstrap 5 |
| ORM | EF Core 10 |
| Database | PostgreSQL **atau** SQL Server |
| Auth | ASP.NET Core Identity + cookie |
| Client boundary | `Inventory.Client` in-process |
| Migrasi | Otomatis saat startup (`DatabaseBootstrapper`) |
| Logging | Built-in ILogger (level default) |

### Topologi Saat Ini

```
Browser
   │ HTTPS (cookie)
   ▼
Inventory.Web (satu proses)
   ├─ Razor MVC
   ├─ Inventory.Client (in-process)
   ├─ Inventory.Application
   └─ EF Core
          │
          ▼
   Satu database PostgreSQL/SQL Server
```

### Lingkungan Deployment

- Satu server aplikasi (VM/on-prem/cloud kecil).
- Database dapat berada di server yang sama atau terpisah.
- Tanpa load balancer, tanpa cache terdistribusi, tanpa queue.
- Backup database bergantung pada konfigurasi server/DB.
- Migrasi dan seeding berjalan setiap aplikasi start (idempotent untuk seeding).

### Ketergantungan Eksternal

- Database PostgreSQL/SQL Server.
- Tidak ada object storage, message broker, atau layanan eksternal lain.
- Tidak ada API publik; UI dan service masih satu host.

### Keterbatasan Lingkungan Saat Ini

1. Belum ada health check/readiness untuk monitoring.
2. Belum ada rate limiting dan response compression.
3. Belum ada distributed cache.
4. Cookie validation melakukan query user ke database setiap request.
5. Data Protection keys belum disiapkan untuk multi-instance.
6. Belum ada metrics/tracing terpusat.
