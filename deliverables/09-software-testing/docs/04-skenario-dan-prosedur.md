# 04 - Skenario dan Prosedur Uji

## 1. Kelompok Skenario

| Kelompok | File test | Jumlah |
|---|---|---:|
| Guard & ledger | `GuardAndLedgerTests`, `CoreTests` | 15+ |
| Stock In/Out | `StockInServiceTests`, `StockOutServiceTests` | 20+ |
| Transfer | `StockTransferServiceTests` | 12 |
| Adjustment/Opname | `StockAdjustmentServiceTests` | 30+ |
| Catalog | `CatalogServiceTests` | 30+ |
| Supplier/Location | `SupplierAndLocationTests` | 25+ |
| Approval/Report/Audit | `ApprovalServiceTests`, `ReportServiceTests`, `DocumentAndAuditTests` | 30+ |
| User/Identity | `UserServiceTests` | 12 |
| **Total** | | **171** |

## 2. Contoh Skenario

| ID | Skenario | Expected |
|---|---|---|
| TC-LED-001 | Apply IN 10 | saldo 10, movement 1 |
| TC-LED-002 | Apply OUT melebihi saldo | `ValidationException` |
| TC-SIN-001 | Create Stock In valid | Draft + detail |
| TC-SIN-002 | Approve oleh non-approver | ditolak |
| TC-SIN-003 | Admin submit | auto-approved |
| TC-SOUT-001 | Submit stok kurang | ditolak |
| TC-SOUT-002 | Approve valid | saldo turun |
| TC-TRF-001 | Transfer valid | asal turun, tujuan naik |
| TC-ADJ-001 | Counted < system | difference negatif |
| TC-OPN-001 | Submit tanpa counted | ditolak |
| TC-CAT-001 | Duplicate SKU | ditolak |
| TC-SUP-001 | Email invalid | ditolak |
| TC-LOC-001 | Parent cycle | ditolak |
| TC-APR-001 | Pending admin | semua tampil |
| TC-RPT-001 | Stock card | running balance benar |
| TC-USR-001 | Last admin demote | ditolak |

## 3. Algoritma Prosedur Uji

```
PROCEDURE RunTest(test):
    ARRANGE:
        create in-memory DB
        seed data
        create fake user/directory
        build service
    ACT:
        call method under test
        capture result/exception
    ASSERT:
        verify result
        verify database state
    CLEANUP:
        dispose DB/connection
END PROCEDURE
```

## 4. Format Eksekusi

```powershell
dotnet test tests/Inventory.UnitTests/Inventory.UnitTests.csproj
```

Dengan TRX + coverage:

```powershell
dotnet test tests/Inventory.UnitTests/Inventory.UnitTests.csproj `
  --logger "trx;LogFileName=test-results.trx" `
  --collect:"XPlat Code Coverage"
```

## 5. Kriteria Lulus

- Semua test Passed.
- Tidak ada skipped.
- State DB sesuai.
- Tidak ada flaky test.
