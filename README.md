# Inventory Web App

Aplikasi web terpusat untuk mengelola persediaan barang: mencatat stok masuk/keluar,
memantau level stok per gudang/cabang, dan menyediakan laporan yang akurat.

## Overview

Inventory Web App adalah aplikasi **ASP.NET Core MVC (.NET 10 LTS)** dengan pendekatan
**Clean Architecture + DDD ringan**. Aplikasi ini menjadi *single source of truth*
untuk data stok dan memastikan setiap perubahan dapat dilacak.

Prinsip utama:

- **Single source of truth** stok per `(Product, Location)`.
- **Audit by default** — setiap mutasi stok menghasilkan `StockMovement` + `AuditLog`; create/update/delete menyimpan ID user pelaku.
- **Soft delete** — master data dan transaksi tidak dihapus permanen.
- **GUID** — ID master data dan transaksi memakai `Guid` (time-ordered).
- **Multi-warehouse / multi-location** — cabang ditandai pada `Location`.
- **Provider-agnostic** — SQL Server atau PostgreSQL dipilih via konfigurasi.
- **Single-level approval** — Staff submit, SPV/Admin approve.

> Dokumentasi perancangan lengkap (ERD, ADR, spesifikasi fitur) ada di
> [docs/README.md](docs/README.md).

## Features

| No | Fitur | Deskripsi | Fase | Status |
|----|-------|-----------|------|--------|
| 01 | Authentication & Authorization | Login/logout, cookie auth, RBAC per role | 1 | Implemented |
| 02 | User & Role Management | Kelola user, role, dan relasi atasan (SPV) | 1 | Implemented |
| 03 | Product & Category Catalog | Master produk, kategori, dan unit | 1 | Implemented |
| 04 | Warehouse & Location | Master lokasi (Branch / Warehouse / Store) | 1 | Implemented |
| 05 | Supplier Management | Master supplier | 2 | Implemented |
| 06 | Stock In (Receiving) | Penerimaan barang + unit cost | 2 | Implemented |
| 07 | Stock Out (Issuing) | Pengeluaran barang | 2 | Implemented |
| 08 | Stock Transfer | Pemindahan stok antar lokasi | 3 | Implemented |
| 09 | Stock Adjustment | Koreksi stok dengan alasan | 3 | Implemented |
| 10 | Stock Opname (Cycle Count) | Penghitungan fisik vs sistem | 3 | Implemented |
| 11 | Dashboard & Report | Dashboard, kartu stok, dan laporan | 4 | Implemented |
| 12 | Audit Trail | Riwayat perubahan entitas | 4 | Implemented |

### Roles

| Role | Hak akses utama |
|------|-----------------|
| `Admin` | Kelola user, role, master data, konfigurasi sistem |
| `SPV` | Approve transaksi, monitoring stok |
| `Staff` | Transaksi stok in/out, transfer, opname |
| `Manager` | Dashboard dan laporan |
| `Auditor` | Akses read-only laporan dan audit trail |

## Tech Stack

| Layer | Pilihan |
|-------|---------|
| Backend | ASP.NET Core (.NET 10 LTS) MVC + Razor |
| Client boundary | `Inventory.Contracts` + `Inventory.Client` (in-process sekarang, HTTP client siap untuk API) |
| View | Razor Views + Bootstrap 5 |
| ORM | EF Core 10 |
| Database | SQL Server **atau** PostgreSQL (via konfigurasi) |
| Auth | ASP.NET Core Identity + Cookie Auth (target: dual cookie + JWT) |
| Testing | xUnit |
| Provider Packages | `Microsoft.EntityFrameworkCore.SqlServer`, `Npgsql.EntityFrameworkCore.PostgreSQL` |

## Architecture

### Alur dependency (transisi menuju API-first)

```
Inventory.Web (Razor UI)
      │
      ▼
Inventory.Client  ──▶  Inventory.Contracts
      │
      ▼ (in-process saat ini)
Inventory.Application ──▶ Inventory.Domain ──▶ Inventory.Contracts
      ▲
Inventory.Infrastructure ──▶ Application/Domain
```

Aturan boundary:
- `Inventory.Web` memakai `Inventory.Client` + `Inventory.Contracts` untuk seluruh
  use case bisnis; tidak boleh memakai entity Domain atau service Application
  secara langsung. Komposisi startup, autentikasi Identity, dan penanganan
  concurrency masih transisi sampai `Inventory.Api` tersedia.
- `Inventory.Client` tidak boleh memakai `Inventory.Infrastructure`.
- `Inventory.Application` / `Inventory.Domain` tidak boleh mereferensikan provider database.
- `Inventory.Api` (target) akan menjadi host REST; `Inventory.Web` cukup mengganti
  implementasi client dari in-process menjadi HTTP.

### Kegunaan tiap layer

| Proyek | Tanggung jawab |
|--------|----------------|
| **Inventory.Domain** | Inti bisnis: entity (`Product`, `Stock`, `StockMovement`, header/detail transaksi), invariant & business rule murni. |
| **Inventory.Contracts** | Kontrak publik lintas boundary: request/response DTO, `PagedResult<T>`, enum publik (`ApprovalStatus`, `MovementType`, `LocationType`, `ReferenceType`), response master data. Tidak ada entity Domain. |
| **Inventory.Application** | Use case per fitur (`StockInService`, `StockOutService`, `StockTransferService`, `StockAdjustmentService`, `StockOpnameService`, `ApprovalService`, `ReportService`, `CatalogService`, `SupplierService`, `LocationService`, `AuditQueryService`), mapping ke `Inventory.Contracts`, abstraksi (`IAppDbContext`, `IUserDirectory`, `IDocumentNumberService`, `IUserService`). Tidak tahu DB apa yang dipakai. |
| **Inventory.Client** | Interface client per area (`ICatalogClient`, `IStockInClient`, dst.) + implementasi in-process. Nanti ditambah implementasi HTTP tanpa mengubah controller Web. |
| **Inventory.Infrastructure** | Implementasi teknis: `AppDbContext`, Identity (`AppUser`, `AppRole`), `UserService`, `DocumentNumberService`, `UserDirectory`, pemilihan provider DB, seeding (`DatabaseBootstrapper`). |
| **Inventory.Migrations.SqlServer** | Migration EF Core khusus SQL Server (DDL provider-specific). |
| **Inventory.Migrations.PostgreSql** | Migration EF Core khusus PostgreSQL. |
| **Inventory.Web** | Presentation: Controller, ViewModel, Razor View, filter otorisasi, konfigurasi startup (`Program.cs`), layout Bootstrap. |
| **Inventory.UnitTests** | Unit test business rule (perhitungan saldo, approval, validasi). |

### Struktur solusi

```
Inventory.sln
├─ src/
│  ├─ Inventory.Domain/
│  ├─ Inventory.Contracts/
│  ├─ Inventory.Application/
│  ├─ Inventory.Client/
│  ├─ Inventory.Infrastructure/
│  ├─ Inventory.Migrations.SqlServer/
│  ├─ Inventory.Migrations.PostgreSql/
│  └─ Inventory.Web/
├─ tests/
│  └─ Inventory.UnitTests/
└─ docs/                     # Dokumentasi perancangan & ADR
```

> **Roadmap pemisahan API/Client:** lihat
> [docs/adr/006-api-first-client-server.md](docs/adr/006-api-first-client-server.md).
> Saat ini Web memakai `Inventory.Client` in-process; tahap berikutnya `Inventory.Api`
> menyediakan REST `/api/v1`, lalu implementasi HTTP di `Inventory.Client` menggantikan
> in-process tanpa perubahan controller Web.

### Pemilihan provider database

Provider dipilih lewat `Database:Provider` (`SqlServer` | `PostgreSql`) di
`appsettings.json`. Registrasi ada di `Infrastructure/DependencyInjection.cs`
dan otomatis memilih migration assembly yang sesuai:

```csharp
if (string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
    options.UseNpgsql(connection, b => b.MigrationsAssembly("Inventory.Migrations.PostgreSql"));
else
    options.UseSqlServer(connection, b => b.MigrationsAssembly("Inventory.Migrations.SqlServer"));
```

Lihat [docs/adr/001-database-provider-strategy.md](docs/adr/001-database-provider-strategy.md).

## Installation & Setup

### 1. Prasyarat

- **.NET 10 SDK** (LTS) — cek dengan `dotnet --version`. Versi SDK dipin melalui `global.json`.
- Salah satu database:
  - **SQL Server** (LocalDB / Express / full), atau
  - **PostgreSQL** 13+.
- (Opsional) `dotnet-ef` CLI untuk mengelola migration manual:
  ```powershell
  dotnet tool install --global dotnet-ef
  ```

### 2. Restore & build

```powershell
dotnet restore
dotnet build
```

### 3. Konfigurasi database

Edit `src/Inventory.Web/appsettings.json`.

**Opsi A — SQL Server LocalDB (default):**

```json
{
  "Database": { "Provider": "SqlServer" },
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=InventoryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Opsi B — SQL Server (user/password):**

```json
{
  "Database": { "Provider": "SqlServer" },
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=InventoryDb;User Id=sa;Password=Your_Password;TrustServerCertificate=True"
  }
}
```

**Opsi C — PostgreSQL:**

```json
{
  "Database": { "Provider": "PostgreSql" },
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=inventorydb;Username=postgres;Password=Your_Password"
  }
}
```

> `Database:ConnectionString` juga didukung sebagai fallback bila
> `ConnectionStrings:Default` tidak diisi.

### 4. Menyiapkan database & menjalankan migrasi

Migrasi **berjalan otomatis saat aplikasi start**: `Program.cs` memanggil
`DatabaseBootstrapper.InitializeAsync`, yang mengeksekusi
`db.Database.MigrateAsync()` lalu seeding (role, user default, sample master data).
Cukup pastikan database target dapat diakses, lalu jalankan aplikasi (langkah 5).

> **Catatan schema:** ID master data & transaksi memakai `Guid` dan entity bisnis
> memakai soft delete. Migration `InitialCreate` sudah merefleksikan schema ini.
> Jika sebelumnya pernah terbentuk database lama, drop database sekali sebelum
> menjalankan aplikasi.

Bila ingin membuat/menerapkan migrasi secara manual:

```powershell
# Membuat migration baru (SQL Server)
dotnet ef migrations add <NamaMigration> `
  --project src/Inventory.Migrations.SqlServer `
  --startup-project src/Inventory.Web

# Membuat migration baru (PostgreSQL)
dotnet ef migrations add <NamaMigration> `
  --project src/Inventory.Migrations.PostgreSql `
  --startup-project src/Inventory.Web

# Menerapkan migration ke database (SQL Server)
dotnet ef database update `
  --project src/Inventory.Migrations.SqlServer `
  --startup-project src/Inventory.Web

# Menerapkan migration ke database (PostgreSQL)
dotnet ef database update `
  --project src/Inventory.Migrations.PostgreSql `
  --startup-project src/Inventory.Web
```

### 5. Menjalankan aplikasi

```powershell
dotnet run --project src/Inventory.Web
```

Aplikasi tersedia di `http://localhost:5201` (lihat
`src/Inventory.Web/Properties/launchSettings.json`).

### 6. Akun default (hasil seeding)

| Username | Role | Password |
|----------|------|----------|
| admin | Admin | `Admin123!` |
| spv | SPV | `Admin123!` |
| staff | Staff | `Admin123!` |
| manager | Manager | `Admin123!` |
| auditor | Auditor | `Admin123!` |

> **Penting:** ganti password default sebelum digunakan di lingkungan nyata.

Sample master data yang ikut di-seed: kategori (`General`, `Consumable`),
unit (`PCS`, `BOX`), lokasi (`HQ`, `HQ-WH1`), produk (`SKU-001`, `SKU-002`),
dan supplier (`SUP-001`).

### Troubleshooting

- **`dotnet ef` tidak dikenali** → install tool global (langkah 1) lalu buka ulang terminal.
- **Koneksi gagal** → pastikan server database berjalan dan connection string benar;
  untuk LocalDB jalankan `sqllocaldb start MSSQLLocalDB`.
- **Error migration saat start** → periksa `Database:Provider` sudah sesuai dengan
  connection string dan migration assembly yang dipakai.

## Testing

```powershell
dotnet test
```

## Dokumentasi

- [docs/README.md](docs/README.md) — index dokumentasi perancangan
- [docs/00-project-overview.md](docs/00-project-overview.md) — visi, aktor, roadmap
- [docs/01-architecture.md](docs/01-architecture.md) — detail arsitektur
- [docs/02-data-model.md](docs/02-data-model.md) — ERD & entitas
- [docs/adr/](docs/adr/) — Architecture Decision Records\n- [docs/adr/005-guid-soft-delete-audit.md](docs/adr/005-guid-soft-delete-audit.md) — GUID, soft delete, audit user
