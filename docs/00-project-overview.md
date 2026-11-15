# 00 - Project Overview

## 1. Visi Produk

Aplikasi web untuk mengelola persediaan (inventory) barang secara terpusat:
mencatat stok masuk/keluar, memantau level stok per gudang, dan menyediakan
laporan yang akurat untuk pengambilan keputusan.

## 2. Tujuan (Goals)

- Menyediakan single source of truth untuk data stok.
- Mencatat setiap mutasi stok beserta jejak audit.
- Mendukung multi-warehouse dan multi-location.
- Menyediakan laporan stok terkini, kartu stok, dan reorder alert.
- Menerapkan **audit by default**: setiap create/update/delete menyimpan user pelaku.
- Menerapkan **soft delete** agar data tidak hilang dari histori.
- Menggunakan **GUID** untuk ID master data dan transaksi.

## 3. Non-Goals (Fase Awal)

- Integrasi marketplace / e-commerce.
- Barcode scanning hardware khusus (disiapkan sebagai future enhancement).
- Multi-tenant / multi-company (dapat ditambahkan kemudian).

## 4. Aktor

| Aktor | Deskripsi |
|-------|-----------|
| Admin | Kelola user, role, master data, dan konfigurasi sistem. |
| Warehouse Staff | Melakukan transaksi stok in/out, transfer, dan opname. |
| Manager | Melihat dashboard, menyetujui transaksi tertentu, dan laporan. |
| Auditor | Akses read-only ke laporan dan audit trail. |

## 5. Tech Stack (Confirmed)

| Layer | Pilihan | Catatan |
|-------|---------|---------|
| Backend | ASP.NET Core (.NET 10 LTS) MVC + Razor | Server-rendered saat ini; disiapkan menjadi API terpisah |
| Client boundary | `Inventory.Contracts` + `Inventory.Client` | Kontrak DTO stabil; client in-process sekarang, HTTP menyusul |
| View | Razor Views + Bootstrap 5 | Tanpa SPA, tanpa build pipeline terpisah |
| ORM | EF Core | Query provider-agnostic |
| Database | SQL Server **atau** PostgreSQL | Dipilih via konfigurasi `Database:Provider` |
| Auth | ASP.NET Core Identity + Cookie Auth | Target: dual cookie + JWT untuk API |
| Testing | xUnit + FluentAssertions | Unit & integration test |
| Deployment | Docker (opsional) | |

## 6. Arsitektur Tingkat Tinggi (High Level)

Target jangka menengah: **API-first** dengan UI sebagai client.

```
Inventory.Web (Razor UI)
      │  memakai Inventory.Client
      ▼
Inventory.Client  ──▶  Inventory.Contracts
      │
      ▼ (saat ini in-process, nanti HTTP /api/v1)
Inventory.Api  ──▶  Inventory.Application ──▶ Inventory.Domain
                           ▲
                  Inventory.Infrastructure (EF Core, Identity)
      ▼
Database
```

Prinsip: Clean Architecture + DDD ringan, SOLID, async/await end-to-end,
serta boundary kontrak yang mencegah UI bergantung pada entity/DB.

Lihat [adr/006-api-first-client-server.md](adr/006-api-first-client-server.md).

## 7. Domain Utama

- **Identity**: User, Role, Permission.
- **Catalog**: Product, Category, Unit, Supplier.
- **Warehouse**: Warehouse, Location.
- **Inventory**: StockItem, StockMovement, Transfer, Adjustment, Opname.
- **Reporting**: Dashboard, Stock Report, Audit Log.

## 8. Roadmap Fase

| Fase | Fokus |
|------|-------|
| 1 | Auth, User/Role, Product & Category, Warehouse |
| 2 | Stock In/Out, Stock Balance, Supplier |
| 3 | Transfer, Adjustment, Opname |
| 4 | Dashboard, Report, Audit Log |

## 9. Keputusan & Asumsi (Resolved)

- **Database:** mendukung SQL Server dan PostgreSQL, dipilih lewat konfigurasi. Tidak ada query provider-specific. Lihat `adr/001-database-provider-strategy.md`.
- **Frontend:** ASP.NET Core MVC + Razor sebagai UI saat ini, diakses melalui boundary `Inventory.Client` + `Inventory.Contracts` agar siap dipisah menjadi API + client/SPA di masa depan. Lihat `adr/002-frontend-mvc.md` dan `adr/006-api-first-client-server.md`.
- **Multi-tenant:** tidak ada. Cabang ditandai pada master **Location**. Lihat `adr/004-location-branch.md`.
- **Approval:** single-level, hanya 1 tahap ke SPV (parent dari person). Lihat `adr/003-single-level-approval.md`.

## 10. Open Questions

- Perlu modul Purchase Order penuh atau cukup Stock In manual?
- Apakah harga (unit cost) wajib dilacak untuk valuasi stok?
