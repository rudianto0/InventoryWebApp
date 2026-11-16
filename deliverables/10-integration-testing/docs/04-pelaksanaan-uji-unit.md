# 04 - Pelaksanaan Uji Modul

## 1. Prosedur

```powershell
dotnet clean Inventory.sln
dotnet build Inventory.sln
dotnet test tests/Inventory.UnitTests/Inventory.UnitTests.csproj `
  --results-directory deliverables/10-integration-testing/results `
  --logger "trx;LogFileName=test-results.trx" `
  --collect:"XPlat Code Coverage"
```

## 2. Modul yang Dijalankan

| Modul | Test |
|---|---|
| Stock Ledger | `GuardAndLedgerTests`, `CoreTests` |
| Stock In | `StockInServiceTests` |
| Stock Out | `StockOutServiceTests` |
| Transfer | `StockTransferServiceTests` |
| Adjustment/Opname | `StockAdjustmentServiceTests` |
| Catalog | `CatalogServiceTests` |
| Supplier/Location | `SupplierAndLocationTests` |
| Approval/Report/Audit | `ApprovalServiceTests`, `ReportServiceTests`, `DocumentAndAuditTests` |
| User/Identity | `UserServiceTests` |

## 3. Implementasi Data/Kondisi

| Kondisi | Cara |
|---|---|
| Valid | seed standar |
| Stok kurang | `SeedStockAsync` qty kecil |
| Non-approver | user id 99 |
| Admin | role `Admin` |
| Soft delete | `IsDeleted=true` |
| Duplikat | input produk sama |

## 4. Hasil

```text
Passed! - Failed: 0, Passed: 171, Skipped: 0, Total: 171
Duration: 7 s
```

## 5. Lembar Hasil

- `sheets/test-case-sheet.csv`
- `sheets/result-sheet.csv`
- `results/test-results.trx`
- `results/test-results.csv`
- `results/test-summary.csv`

## 6. Catatan

Pengujian modul dilakukan pada level service + `AppDbContext` (SQLite).
Belum termasuk HTTP/UI/PostgreSQL nyata.
