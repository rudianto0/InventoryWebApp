# 01 - Lingkup dan Arsitektur Database

## 1. Lingkup

Database mendukung seluruh modul aplikasi Inventory:

- Identity dan otorisasi (`Users`, `Roles`, `UserRoles`, dll.).
- Master data: kategori, unit, produk, supplier, lokasi.
- Persediaan: saldo stok dan movement.
- Transaksi: Stock In, Stock Out, Transfer, Adjustment, Opname.
- Approval satu level.
- Audit log dan soft delete.
- Penomoran dokumen.

Di luar lingkup: integrasi marketplace, multi-tenant, dan multi-company.

## 2. Arsitektur Database

```
Aplikasi ASP.NET Core (EF Core)
        │
        ▼
PostgreSQL - InventoryAppDB
├─ Master data
├─ Stocks & StockMovements (inti persediaan)
├─ Header/Detail transaksi (5 tipe)
├─ DocumentSequences
├─ AuditLogs
└─ Identity (Users/Roles/...)
```

Prinsip:

- `Stocks` adalah agregat saldo per `(ProductId, LocationId)`.
- Setiap perubahan saldo menghasilkan `StockMovements`.
- Header/detail transaksi menyimpan dokumen bisnis.
- Soft delete diterapkan pada master dan transaksi.
- Concurrency token `Stocks.Version` mencegah lost update.

## 3. Tabel Utama

### Master
| Tabel | Fungsi |
|---|---|
| `Categories` | Kategori produk |
| `Units` | Satuan |
| `Products` | Produk + reorder level |
| `Suppliers` | Supplier |
| `Locations` | Branch/Warehouse/Store + hierarki |

### Persediaan
| Tabel | Fungsi |
|---|---|
| `Stocks` | Saldo per produk/lokasi |
| `StockMovements` | Ledger mutasi (in/out/transfer/adjustment/opname) |

### Transaksi
| Tabel | Fungsi |
|---|---|
| `StockInHeaders` / `StockInDetails` | Penerimaan |
| `StockOutHeaders` / `StockOutDetails` | Pengeluaran |
| `StockTransferHeaders` / `StockTransferDetails` | Transfer |
| `StockAdjustmentHeaders` / `StockAdjustmentDetails` | Koreksi |
| `StockOpnameHeaders` / `StockOpnameDetails` | Opname |

### Sistem
| Tabel | Fungsi |
|---|---|
| `DocumentSequences` | Nomor dokumen per prefix/bulan |
| `AuditLogs` | Jejak audit |
| `Users`, `Roles`, `UserRoles`, ... | Identity |

## 4. Konvensi

- Nama tabel dan kolom **PascalCase** dan harus di-quote (`"Products"`).
- Enum disimpan sebagai `integer`:
  - `LocationType`: Branch=0, Warehouse=1, Store=2.
  - `ApprovalStatus`: Draft=0, Submitted=1, Approved=2, Rejected=3, Cancelled=4.
  - `MovementType`: In=0, Out=1, TransferOut=2, TransferIn=3, Adjustment=4, Opname=5.
  - `ReferenceType`: StockIn=0, StockOut=1, StockTransfer=2, StockAdjustment=3, StockOpname=4.
- Quantity `integer`; `UnitCost numeric(18,4)`.
- Timestamp `timestamp with time zone`.
- ID bisnis `uuid`; ID user `integer`.

## 5. Integritas

- FK utama memakai `ON DELETE RESTRICT`; detail transaksi memakai `CASCADE`.
- Unique index aktif memakai filter `IsDeleted = false`.
- `Stocks` unik pada `(ProductId, LocationId)`.
- Index tambahan disediakan untuk laporan dan audit.
