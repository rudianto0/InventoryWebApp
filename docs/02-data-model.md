# 02 - Data Model (ERD & Entities)

Semua ID data bisnis menggunakan `Guid` (`Guid.CreateVersion7()`, time-ordered).
ID user tetap `int` karena terikat ASP.NET Identity. Semua timestamp `DateTime`
(UTC). Quantity menggunakan `int`, cost menggunakan `decimal(18,4)`. Tidak ada `TenantId`. Cabang ditandai
pada `Locations` (lihat ADR-004).

Semua entity bisnis memiliki **audit fields**:

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `CreatedAt` | DateTime (UTC) | Ya | Otomatis saat insert | Waktu record dibuat |
| `CreatedById` | int? | Tidak | Otomatis dari sesi; null saat seeding | ID user pembuat |
| `UpdatedAt` | DateTime? | Tidak | Otomatis saat update | Waktu terakhir diubah |
| `UpdatedById` | int? | Tidak | Otomatis dari sesi | ID user terakhir yang mengubah |
| `IsDeleted` | bool | Ya | Default `false` | Penanda soft delete |
| `DeletedAt` | DateTime? | Tidak | Otomatis saat delete | Waktu dihapus |
| `DeletedById` | int? | Tidak | Otomatis dari sesi | ID user yang menghapus |

Aturan audit:
- Field user (`CreatedById`, `UpdatedById`, `DeletedById`, `ApproverId`, dll.)
  bertipe `int?` dan **bukan FK fisik** ke `Users` karena layer Domain tidak
  boleh bergantung pada Identity. Integritas dijaga di application layer + index.
- `AppUser` hanya memiliki audit create/update (tanpa soft delete) karena user
  dinonaktifkan lewat `IsActive`.

## 1. Ringkasan Entitas

### Identity

| Entity | Key Fields |
|--------|-----------|
| `Users` | Id (int), UserName, Email, FullName, PasswordHash, **ParentId (SPV)**, IsActive, CreatedAt, CreatedById, UpdatedAt, UpdatedById |
| `Roles` | Id (int), Name, Description |

- `Users.ParentId` → self-reference ke `Users.Id` (atasan/SPV). Nullable untuk top-level (mis. Admin).
- Role default: `Admin`, `SPV`, `Staff`, `Manager`, `Auditor`.

### Master Data (PK `Guid`, soft delete)

| Entity | Fields |
|--------|--------|
| `Categories` | Id, Name, Description, IsActive, audit, soft delete |
| `Units` | Id, Code, Name, audit, soft delete |
| `Products` | Id, Sku, Name, CategoryId, UnitId, ReorderLevel, IsActive, audit, soft delete |
| `Suppliers` | Id, Code, Name, ContactPerson, Phone, Email, Address, IsActive, audit, soft delete |
| `Locations` | Id, Code, Name, **Type**, **ParentLocationId**, **BranchCode**, Address, IsActive, audit, soft delete |

- `Locations.Type` ∈ `{ Branch, Warehouse, Store }`.
- `Locations.ParentLocationId` → self-reference (Cabang → Gudang).
- Unique (filtered `IsDeleted = false`): `Categories.Name`, `Units.Code`,
  `Products.Sku`, `Suppliers.Code`, `Locations.Code`.

#### Field Dictionary Master Data

| Entity | Field | Type | Required | Constraint / Default | Fungsi |
|--------|-------|------|----------|----------------------|--------|
| Category | `Id` | Guid | Ya | PK | Identitas unik kategori |
| Category | `Name` | string(150) | Ya | Unik (aktif) | Nama kategori |
| Category | `Description` | string(500) | Tidak | - | Deskripsi kategori |
| Category | `IsActive` | bool | Ya | Default `true` | Status bisnis kategori |
| Unit | `Id` | Guid | Ya | PK | Identitas unik satuan |
| Unit | `Code` | string(30) | Ya | Unik (aktif) | Kode satuan (mis. PCS) |
| Unit | `Name` | string(100) | Ya | - | Nama satuan |
| Product | `Id` | Guid | Ya | PK | Identitas unik produk |
| Product | `Sku` | string(50) | Ya | Unik (aktif), uppercase, tanpa spasi | Kode stok unik |
| Product | `Name` | string(200) | Ya | - | Nama produk |
| Product | `CategoryId` | Guid | Ya | FK Categories | Kategori produk |
| Product | `UnitId` | Guid | Ya | FK Units | Satuan produk |
| Product | `ReorderLevel` | int | Ya | `>= 0` | Ambang batas reorder |
| Product | `IsActive` | bool | Ya | Default `true` | Status bisnis produk |
| Supplier | `Id` | Guid | Ya | PK | Identitas unik supplier |
| Supplier | `Code` | string(30) | Ya | Unik (aktif) | Kode supplier |
| Supplier | `Name` | string(150) | Ya | - | Nama supplier |
| Supplier | `ContactPerson` | string(150) | Tidak | - | Nama kontak |
| Supplier | `Phone` | string(30) | Tidak | - | Nomor telepon |
| Supplier | `Email` | string(150) | Tidak | Format email valid | Email kontak |
| Supplier | `Address` | string(300) | Tidak | - | Alamat supplier |
| Supplier | `IsActive` | bool | Ya | Default `true` | Status bisnis supplier |
| Location | `Id` | Guid | Ya | PK | Identitas unik lokasi |
| Location | `Code` | string(30) | Ya | Unik (aktif) | Kode lokasi |
| Location | `Name` | string(150) | Ya | - | Nama lokasi |
| Location | `Type` | enum | Ya | Branch/Warehouse/Store | Tipe lokasi |
| Location | `ParentLocationId` | Guid? | Tidak | FK Locations | Induk hierarki |
| Location | `BranchCode` | string(30) | Tidak | - | Kode cabang untuk laporan |
| Location | `Address` | string(300) | Tidak | - | Alamat lokasi |
| Location | `IsActive` | bool | Ya | Default `true` | Status bisnis lokasi |

### Inventory (PK `Guid`)

| Entity | Fields |
|--------|--------|
| `Stocks` | Id, ProductId, LocationId, Quantity, UpdatedAt, Version, audit |
| `StockMovements` | Id, MovementNo, Type, ProductId, LocationId, Quantity (signed), ReferenceType, ReferenceId, Notes, audit |

- `Stocks` **tidak** memakai soft delete; saldo adalah data turunan dari ledger.
- `StockMovements` **tidak** memakai soft delete; ledger append-only.
- `MovementType` ∈ `{ In, Out, TransferOut, TransferIn, Adjustment, Opname }`.
- `ReferenceId` bertipe `Guid` dan menunjuk ke header transaksi.

#### Field Dictionary Inventory

| Entity | Field | Type | Required | Constraint / Default | Fungsi |
|--------|-------|------|----------|----------------------|--------|
| Stock | `Id` | Guid | Ya | PK | Identitas baris saldo |
| Stock | `ProductId` | Guid | Ya | FK Products | Produk saldo |
| Stock | `LocationId` | Guid | Ya | FK Locations | Lokasi saldo |
| Stock | `Quantity` | int | Ya | `>= 0` | Saldo terkini |
| Stock | `UpdatedAt` | DateTime | Ya | Otomatis | Waktu update saldo |
| Stock | `Version` | int | Ya | Concurrency token | Optimistic concurrency |
| StockMovement | `Id` | Guid | Ya | PK | Identitas mutasi |
| StockMovement | `MovementNo` | string(50) | Ya | Unik | Nomor mutasi |
| StockMovement | `Type` | enum | Ya | MovementType | Jenis mutasi |
| StockMovement | `ProductId` | Guid | Ya | FK Products | Produk yang bergerak |
| StockMovement | `LocationId` | Guid | Ya | FK Locations | Lokasi mutasi |
| StockMovement | `Quantity` | int | Ya | Signed | Delta kuantitas |
| StockMovement | `ReferenceType` | enum | Ya | ReferenceType | Jenis dokumen sumber |
| StockMovement | `ReferenceId` | Guid | Ya | - | ID header dokumen sumber |
| StockMovement | `Notes` | string(500) | Tidak | - | Catatan mutasi |

### Transactions (header/detail, PK `Guid`, soft delete)

Semua header transaksi mengimplementasikan kontrak approval (ADR-003):
`Status, SubmittedAt, ApproverId, ApprovedAt, ApprovedById, RejectionReason`.
Semua header/detail memiliki audit fields dan soft delete.

| Header | Detail | Catatan |
|--------|--------|---------|
| `StockInHeaders` | `StockInDetails` | SupplierId, LocationId, DocNo, Date |
| `StockOutHeaders` | `StockOutDetails` | LocationId, DocNo, Date, Purpose |
| `StockTransferHeaders` | `StockTransferDetails` | FromLocationId, ToLocationId, DocNo, Date |
| `StockAdjustmentHeaders` | `StockAdjustmentDetails` | LocationId, DocNo, Date, Reason |
| `StockOpnameHeaders` | `StockOpnameDetails` | LocationId, DocNo, Date |

Detail umum: Id, HeaderId, ProductId, Quantity, Notes.
Khusus:
- `StockInDetails.UnitCost` (nullable, untuk valuasi).
- `StockAdjustmentDetails.SystemQuantity`, `CountedQuantity`, `Difference`.
- `StockOpnameDetails.SystemQuantity`, `CountedQuantity`, `Difference`.

#### Field Dictionary Header Transaksi

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas dokumen |
| `DocNo` | string(30) | Ya | Unik (aktif) | Nomor dokumen otomatis |
| `Date` | DateTime | Ya | - | Tanggal transaksi |
| `Status` | enum | Ya | Default Draft | Status approval |
| `SubmittedAt` | DateTime? | Tidak | - | Waktu submit |
| `ApproverId` | int? | Tidak | - | ID user approver |
| `ApprovedAt` | DateTime? | Tidak | - | Waktu approve |
| `ApprovedById` | int? | Tidak | - | ID user yang approve |
| `RejectionReason` | string? | Tidak | - | Alasan reject |
| `CreatedAt` / `CreatedById` | DateTime / int? | Ya / Tidak | Otomatis | Audit pembuatan |
| `UpdatedAt` / `UpdatedById` | DateTime? / int? | Tidak | Otomatis | Audit perubahan |
| `IsDeleted` | bool | Ya | Default `false` | Soft delete |
| `DeletedAt` / `DeletedById` | DateTime? / int? | Tidak | Otomatis | Audit penghapusan |
| `Notes` | string(500) | Tidak | - | Catatan dokumen |

Field khusus header: `SupplierId` (Stock In), `LocationId`, `FromLocationId`
dan `ToLocationId` (Transfer), `Purpose` (Stock Out), `Reason` (Adjustment).

#### Field Dictionary Detail Transaksi

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas baris |
| `{Header}Id` | Guid | Ya | FK header | Dokumen induk |
| `ProductId` | Guid | Ya | FK Products | Produk pada baris |
| `Quantity` | int | Ya | `> 0` | Kuantitas transaksi |
| `UnitCost` | decimal(18,4)? | Tidak | `>= 0` | Harga satuan (Stock In) |
| `SystemQuantity` | int | Ya | - | Stok sistem saat snapshot |
| `CountedQuantity` | int? | Tidak | `>= 0` | Hasil hitung fisik |
| `Difference` | int | Ya | - | `Counted - System` |
| `Notes` | string(500) | Tidak | - | Catatan baris |
| audit + `IsDeleted` | - | - | Otomatis | Jejak create/update/delete |

### Audit

| Entity | Fields |
|--------|--------|
| `AuditLogs` | Id (int), EntityName, EntityId (**string(64)**), Action, OldValues (JSON string), NewValues (JSON string), UserId (int?), IpAddress, CreatedAt |

#### Field Dictionary AuditLog

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | int | Ya | PK | Identitas log |
| `EntityName` | string(100) | Ya | - | Nama entity yang berubah |
| `EntityId` | string(64) | Ya | `Guid`/`int` sebagai string | ID record yang berubah |
| `Action` | string(30) | Ya | Create/Update/Delete/Submit/Approve/Reject | Jenis aksi |
| `OldValues` | string? | Tidak | JSON | Nilai sebelum perubahan |
| `NewValues` | string? | Tidak | JSON | Nilai sesudah perubahan |
| `UserId` | int? | Tidak | - | User pelaku |
| `IpAddress` | string(50) | Tidak | - | IP pelaku |
| `CreatedAt` | DateTime | Ya | Otomatis | Waktu log |

## 2. Relasi (Textual ERD)

```
Roles 1 ── * Users
Users 1 ── * Users            (ParentId → SPV, self-reference)

Categories 1 ── * Products
Units      1 ── * Products
Products   1 ── * Stocks * ── 1 Locations
Locations  1 ── * Locations   (ParentLocationId, self-reference)

StockInHeaders 1 ── * StockInDetails * ── 1 Products
StockInHeaders * ── 1 Suppliers
StockInHeaders * ── 1 Locations

StockOutHeaders 1 ── * StockOutDetails * ── 1 Products
StockTransferHeaders 1 ── * StockTransferDetails * ── 1 Products
StockAdjustmentHeaders 1 ── * StockAdjustmentDetails * ── 1 Products
StockOpnameHeaders 1 ── * StockOpnameDetails * ── 1 Products

StockMovements * ── 1 Products
StockMovements * ── 1 Locations
StockMovements * ── 1 Users (CreatedById)
```

## 3. Enum

```csharp
public enum LocationType { Branch, Warehouse, Store }
public enum ApprovalStatus { Draft, Submitted, Approved, Rejected, Cancelled }
public enum MovementType { In, Out, TransferOut, TransferIn, Adjustment, Opname }
public enum ReferenceType { StockIn, StockOut, StockTransfer, StockAdjustment, StockOpname }
```

## 4. Index & Constraint Penting

| Tabel | Constraint |
|-------|-----------|
| Users | Unique(UserName), Unique(Email) |
| Categories | Unique(Name) dengan filter `IsDeleted = false` |
| Units | Unique(Code) dengan filter `IsDeleted = false` |
| Products | Unique(Sku) dengan filter `IsDeleted = false`, Index(CategoryId) |
| Suppliers | Unique(Code) dengan filter `IsDeleted = false` |
| Locations | Unique(Code) dengan filter `IsDeleted = false`, Index(ParentLocationId), Index(BranchCode) |
| Stocks | Unique(ProductId, LocationId), Check(Quantity >= 0) |
| StockMovements | Index(ProductId, LocationId, CreatedAt) |
| All Headers | Unique(DocNo) dengan filter `IsDeleted = false`, Index(Status), Index(ApproverId) |

Filtered index dipilih sesuai provider: SQL Server `[IsDeleted] = 0`,
PostgreSQL `"IsDeleted" = false`, SQLite `IsDeleted = 0`.

## 5. Aturan Saldo (Stocks)

- `Quantity` adalah saldo terkini per produk per lokasi.
- Nilai awal = 0, dibuat saat produk/lokasi dipakai pertama kali.
- Perubahan hanya melalui service yang juga menulis `StockMovements`.
- `Check(Quantity >= 0)`; bila kebijakan mengizinkan minus, constraint dihapus.

## 6. Aturan Soft Delete

- Semua master data dan header/detail transaksi memakai soft delete.
- `Stocks` dan `StockMovements` **tidak** di-soft-delete (ledger append-only).
- Global query filter `!IsDeleted` diterapkan otomatis oleh `AppDbContext`.
- Master yang sudah direferensikan tidak boleh dihapus; gunakan `IsActive = false`.
- Delete transaksi tetap hanya untuk status `Draft`; data yang dihapus tetap
  tersimpan dengan `IsDeleted = true`, `DeletedAt`, dan `DeletedById`.

## 7. Document Number Format

`{PREFIX}-{yyyyMM}-{seq:0000}`

| Prefix | Dokumen |
|--------|---------|
| IN | Stock In |
| OUT | Stock Out |
| TRF | Stock Transfer |
| ADJ | Stock Adjustment |
| OPN | Stock Opname |

Sequence di-reset per bulan per prefix, dijaga di tabel `DocumentSequences`
(`Prefix, Year, Month, LastNumber`) dengan transaksi atomik. `DocumentSequences`
tetap memakai PK `int` karena merupakan data sistem internal.