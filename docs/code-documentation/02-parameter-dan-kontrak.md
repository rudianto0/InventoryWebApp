# 02 - Parameter dan Kontrak

## 1. Konvensi Parameter

- `ct` = `CancellationToken` untuk pembatalan.
- Parameter wajib tidak nullable; opsional memakai `?`/default.
- ID memakai `Guid` (bisnis) atau `int` (user).
- Quantity `int`, biaya `decimal?`.
- Semua input divalidasi di service sebelum persist.

## 2. DTO Input

### Stock In
| Parameter | Tipe | Nullable | Validasi |
|---|---|---|---|
| `SupplierId` | `Guid` | Tidak | harus ada & aktif |
| `LocationId` | `Guid` | Tidak | harus ada & aktif |
| `Date` | `DateTime` | Tidak | default UTC |
| `Notes` | `string?` | Ya | max 500 |
| `Lines` | `List<DocumentLineInput>` | Tidak | minimal 1 |
| `Lines[].ProductId` | `Guid` | Tidak | unik, aktif |
| `Lines[].Quantity` | `int` | Tidak | > 0 |
| `Lines[].UnitCost` | `decimal?` | Ya | ≥ 0 |

### Stock Out
| Parameter | Tipe | Nullable | Validasi |
|---|---|---|---|
| `LocationId` | `Guid` | Tidak | ada & aktif |
| `Purpose` | `string?` | Ya | max 200 |
| `Lines` | `List<DocumentLineInput>` | Tidak | minimal 1, qty > 0 |
| stok | - | - | harus cukup saat submit/approve |

### Transfer
| Parameter | Tipe | Nullable | Validasi |
|---|---|---|---|
| `FromLocationId` | `Guid` | Tidak | ada, aktif |
| `ToLocationId` | `Guid` | Tidak | ada, aktif, ≠ asal |
| `Lines` | `List<DocumentLineInput>` | Tidak | minimal 1, qty > 0 |
| stok asal | - | - | harus cukup |

### Adjustment
| Parameter | Tipe | Nullable | Validasi |
|---|---|---|---|
| `Reason` | `string` | Tidak | wajib |
| `LocationId` | `Guid` | Tidak | ada & aktif |
| `Lines[].CountedQuantity` | `int` | Tidak | ≥ 0 |

### Opname
| Parameter | Tipe | Nullable | Validasi |
|---|---|---|---|
| `LocationId` | `Guid` | Tidak | ada, memiliki stok |
| `Lines[].DetailId` | `Guid` | Tidak | milik header |
| `Lines[].CountedQuantity` | `int?` | Ya | ≥ 0 |

## 3. DTO Filter

| DTO | Field | Fungsi |
|---|---|---|
| `TransactionFilter` | `Status`, `From`, `To`, `Page`, `PageSize` | filter daftar transaksi |
| `ReportFilter` | `LocationId`, `BranchCode`, `ProductId`, `From`, `To`, `Page`, `PageSize` | filter laporan |
| `AuditFilter` | `EntityName`, `Action`, `UserId`, `From`, `To`, `Page`, `PageSize` | filter audit |
| `UserFilter` | `Search`, `RoleName`, `IsActive`, `Page`, `PageSize` | filter user |

## 4. Parameter Method Inti

| Method | Parameter | Return |
|---|---|---|
| `StockLedger.ApplyAsync` | `IEnumerable<StockChange> changes`, `ct` | `Task` |
| `StockInService.CreateAsync` | `StockInInput`, `ct` | `Task<TransactionCreatedResponse>` |
| `StockInService.SubmitAsync` | `Guid id`, `ct` | `Task` |
| `StockInService.ApproveAsync` | `Guid id`, `string? notes`, `ct` | `Task` |
| `ApprovalGuard.SubmitAsync` | `IApprovable`, `int`, `IUserDirectory`, `ct` | `Task` |
| `Guard.Positive` | `int/decimal value`, `string field` | `void` |
| `DocumentNumberService.NextAsync` | `string prefix`, `DateTime date`, `ct` | `Task<string>` |
| `ReportService.GetStockBalanceAsync` | `ReportFilter`, `ct` | `Task<PagedResult<StockBalanceItem>>` |

## 5. Contoh Pemanggilan

```csharp
var input = new StockInInput
{
    SupplierId = supplierId,
    LocationId = locationId,
    Date = DateTime.UtcNow,
    Lines = new List<DocumentLineInput>
    {
        new() { ProductId = productId, Quantity = 10, UnitCost = 45000m }
    }
};

var created = await stockInService.CreateAsync(input, ct);
await stockInService.SubmitAsync(created.Id, ct);
```
