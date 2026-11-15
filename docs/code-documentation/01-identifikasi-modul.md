# 01 - Identifikasi Modul Program

## 1. Daftar Modul

| Module ID | Modul | File utama | Jenis |
|---|---|---|---|
| `MOD-DOM-CATALOG` | Entity katalog | `Domain/Entities/Catalog.cs` | Entity |
| `MOD-DOM-INVENTORY` | Stok & movement | `Domain/Entities/Inventory.cs` | Entity |
| `MOD-DOM-TRANSACTION` | Header/detail transaksi | `Domain/Entities/Transactions.cs` | Entity |
| `MOD-DOM-LOCATION` | Lokasi | `Domain/Entities/Location.cs` | Entity |
| `MOD-DOM-SYSTEM` | Audit/sequence/identity | `Domain/Entities/SystemEntities.cs` | Entity |
| `MOD-APP-CATALOG` | Katalog | `Application/Services/CatalogService.cs` | Service |
| `MOD-APP-SUPPLIER` | Supplier | `Application/Services/SupplierService.cs` | Service |
| `MOD-APP-LOCATION` | Lokasi | `Application/Services/LocationService.cs` | Service |
| `MOD-APP-LEDGER` | Mutasi stok | `Application/Services/StockLedger.cs` | Service/Procedure |
| `MOD-APP-STOCKIN` | Stock In | `Application/Services/StockInService.cs` | Service |
| `MOD-APP-STOCKOUT` | Stock Out | `Application/Services/StockOutService.cs` | Service |
| `MOD-APP-TRANSFER` | Transfer | `Application/Services/StockTransferService.cs` | Service |
| `MOD-APP-ADJUSTMENT` | Adjustment | `Application/Services/StockAdjustmentService.cs` | Service |
| `MOD-APP-OPNAME` | Opname | `Application/Services/StockOpnameService.cs` | Service |
| `MOD-APP-APPROVAL` | Approval | `ApprovalService.cs`, `ApprovalGuard` | Service |
| `MOD-APP-REPORT` | Laporan | `Application/Services/ReportService.cs` | Service |
| `MOD-APP-AUDIT` | Audit query | `Application/Services/AuditQueryService.cs` | Service |
| `MOD-APP-GUARD` | Validasi | `Application/Common/Guard.cs` | Utility |
| `MOD-INF-DBCONTEXT` | EF Core/audit/soft delete | `Infrastructure/Persistence/AppDbContext.cs` | Infrastructure |
| `MOD-INF-DOCNUM` | Nomor dokumen | `Infrastructure/Services/DocumentNumberService.cs` | Infrastructure |
| `MOD-INF-USER` | Identity/user | `UserService.cs`, `UserDirectory.cs` | Infrastructure |
| `MOD-CLI-CLIENT` | Client boundary | `Inventory.Client/*` | Client |
| `MOD-WEB-UI` | Controller & views | `Inventory.Web/*` | Presentation |
| `MOD-TST-UNIT` | Unit test | `tests/Inventory.UnitTests/*` | Test |

## 2. Struktur Layer

```
Domain (entity, invariant)
   ▲
Application (use case, validasi, DTO)
   ▲
Infrastructure (EF Core, Identity, sequence)
   ▲
Client (kontrak + in-process)
   ▲
Web (controller, Razor, ViewModel)
```

## 3. Aturan Identitas

- Format Module ID: `MOD-<LAYER>-<NAME>`.
- Format Method ID: `MTH-<LAYER>-<MODULE>-<ACTION>`.
- Format Procedure ID: `PRC-...`.
- Format Function ID: `FN-...`.
- Setiap ID harus muncul di dokumen modul, method, dan traceability.
