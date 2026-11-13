# 02 - Data Model (ERD & Entities)

Semua ID bertipe `int`. Semua timestamp `DateTime` (UTC). Quantity & cost `decimal(18,4)`.
Tidak ada `TenantId`. Cabang ditandai pada `Locations` (lihat ADR-004).

## 1. Ringkasan Entitas

### Identity

| Entity | Key Fields |
|--------|-----------|
| `Users` | Id, UserName, Email, FullName, PasswordHash, RoleId, **ParentId (SPV)**, IsActive, CreatedAt |
| `Roles` | Id, Name, Description |

- `Users.ParentId` → self-reference ke `Users.Id` (atasan/SPV). Nullable untuk top-level (mis. Admin).
- Role default: `Admin`, `SPV`, `Staff`, `Manager`, `Auditor`.

### Master Data

| Entity | Fields |
|--------|--------|
| `Categories` | Id, Name, Description, IsActive |
| `Units` | Id, Code, Name |
| `Products` | Id, Sku, Name, CategoryId, UnitId, ReorderLevel, IsActive, CreatedAt |
| `Suppliers` | Id, Code, Name, ContactPerson, Phone, Email, Address, IsActive |
| `Locations` | Id, Code, Name, **Type**, **ParentLocationId**, **BranchCode**, Address, IsActive |

- `Locations.Type` ∈ `{ Branch, Warehouse, Store }`.
- `Locations.ParentLocationId` → self-reference (Cabang → Gudang).
- Unique: `Products.Sku`, `Suppliers.Code`, `Locations.Code`.

### Inventory

| Entity | Fields |
|--------|--------|
| `Stocks` | Id, ProductId, LocationId, Quantity, UpdatedAt — **Unique(ProductId, LocationId)** |
| `StockMovements` | Id, MovementNo, Type, ProductId, LocationId, Quantity (signed), ReferenceType, ReferenceId, Notes, CreatedAt, CreatedById |

- `MovementType` ∈ `{ In, Out, TransferOut, TransferIn, Adjustment, Opname }`.

### Transactions (header/detail)

Semua header transaksi mengimplementasikan kontrak approval (ADR-003):
`Status, SubmittedAt, ApproverId, ApprovedAt, ApprovedById, RejectionReason, CreatedById, CreatedAt`.

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

### Audit

| Entity | Fields |
|--------|--------|
| `AuditLogs` | Id, EntityName, EntityId, Action, OldValues (JSON string), NewValues (JSON string), UserId, IpAddress, CreatedAt |

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
| Products | Unique(Sku), Index(CategoryId) |
| Locations | Unique(Code), Index(ParentLocationId), Index(BranchCode) |
| Stocks | Unique(ProductId, LocationId), Check(Quantity >= 0) |
| StockMovements | Index(ProductId, LocationId, CreatedAt) |
| All Headers | Unique(DocNo), Index(Status), Index(ApproverId) |

## 5. Aturan Saldo (Stocks)

- `Quantity` adalah saldo terkini per produk per lokasi.
- Nilai awal = 0, dibuat saat produk/lokasi dipakai pertama kali.
- Perubahan hanya melalui service yang juga menulis `StockMovements`.
- `Check(Quantity >= 0)`; bila kebijakan mengizinkan minus, constraint dihapus.

## 6. Document Number Format

`{PREFIX}-{yyyyMM}-{seq:0000}`

| Prefix | Dokumen |
|--------|---------|
| IN | Stock In |
| OUT | Stock Out |
| TRF | Stock Transfer |
| ADJ | Stock Adjustment |
| OPN | Stock Opname |

Sequence di-reset per bulan per prefix, dijaga di tabel `DocumentSequences`
(`Prefix, Year, Month, LastNumber`) dengan transaksi atomik.
