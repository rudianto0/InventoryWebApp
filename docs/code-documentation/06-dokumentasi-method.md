# 06 - Dokumentasi Method, Fungsi, dan Prosedur

## 1. Format Entri

| Field | Keterangan |
|---|---|
| Method ID | `MTH/FN/PRC-...` |
| Signature | Nama + parameter |
| Kategori | Fungsi / Prosedur |
| Modul | Module ID |
| Parameter | Tipe, nullable, arti |
| Return | Tipe dan arti |
| Eksepsi | Exception yang mungkin |
| Efek samping | Perubahan state/DB |
| Contoh | Pemanggilan |

## 2. Method Inti

### MTH-APP-LEDGER-APPLY — `StockLedger.ApplyAsync`
- Kategori: Prosedur.
- Parameter: `IEnumerable<StockChange> changes`, `CancellationToken ct`.
- Return: `Task`.
- Efek: update `Stocks`, insert `StockMovements`.
- Eksepsi: `ValidationException` jika saldo negatif.
- Modul: `MOD-APP-LEDGER`.

### MTH-APP-STOCKIN-CREATE — `StockInService.CreateAsync`
- Kategori: Prosedur.
- Parameter: `StockInInput input`, `CancellationToken ct`.
- Return: `TransactionCreatedResponse`.
- Efek: insert header/detail Draft.
- Eksepsi: `ValidationException`.

### MTH-APP-STOCKIN-SUBMIT — `StockInService.SubmitAsync`
- Parameter: `Guid id`, `CancellationToken ct`.
- Efek: status Submitted / auto-approve admin.
- Eksepsi: `ValidationException`.

### MTH-APP-STOCKIN-APPROVE — `StockInService.ApproveAsync`
- Parameter: `Guid id`, `string? notes`, `CancellationToken ct`.
- Efek: status Approved + stok.
- Eksepsi: `ValidationException`.

### MTH-APP-STOCKOUT-VALIDATE — `StockOutService.ValidateLinesAsync`
- Kategori: Fungsi validasi.
- Parameter: `productIds`, `locationId`, `stockLines`, `ct`.
- Return: `Task`.
- Eksepsi: duplikat/nonaktif/stok kurang.

### MTH-APP-APPROVAL-SUBMIT — `ApprovalGuard.SubmitAsync`
- Kategori: Prosedur.
- Parameter: `IApprovable`, `int`, `IUserDirectory`, `ct`.
- Efek: set `ApproverId`, `SubmittedAt`, `Status`.
- Eksepsi: atasan tidak ada/self/nonaktif.

### MTH-APP-APPROVAL-CAN — `ApprovalGuard.EnsureCanApprove`
- Kategori: Fungsi otorisasi.
- Parameter: `IApprovable`, `createdById`, `currentUserId`, `isAdmin`.
- Return: `void`.
- Eksepsi: approve sendiri/status salah/tanpa approver/bukan approver.

### MTH-APP-GUARD-POSITIVE — `Guard.Positive`
- Kategori: Fungsi validasi.
- Parameter: `int/decimal value`, `string field`.
- Return: `void`.
- Eksepsi: `ValidationException`.

### MTH-INF-DOCNUM-NEXT — `DocumentNumberService.NextAsync`
- Kategori: Fungsi/prosedur.
- Parameter: `prefix`, `date`, `ct`.
- Return: `Task<string>`.
- Efek: increment `DocumentSequences`.
- Eksepsi: `DbUpdateException`.

### MTH-INF-DBCONTEXT-SAVE — `AppDbContext.SaveChangesAsync`
- Kategori: Prosedur.
- Parameter: `CancellationToken`.
- Return: `Task<int>`.
- Efek: audit, soft delete, audit log.
- Eksepsi: `DbUpdateException`, `DbUpdateConcurrencyException`.

### MTH-APP-REPORT-BALANCE — `ReportService.GetStockBalanceAsync`
- Kategori: Fungsi.
- Parameter: `ReportFilter`, `ct`.
- Return: `Task<PagedResult<StockBalanceItem>>`.
- Efek: read-only.

### MTH-APP-REPORT-CARD — `ReportService.GetStockCardAsync`
- Kategori: Fungsi.
- Parameter: `ReportFilter`, `ct`.
- Return: `Task<IReadOnlyList<StockCardItem>>`.
- Catatan: perlu pagination untuk histori besar.

### MTH-INF-USER-CREATE — `UserService.CreateAsync`
- Kategori: Prosedur.
- Parameter: `UserInput`, `ct`.
- Return: `Task<int>`.
- Eksepsi: validasi field, password, role, duplikat, parent.

## 3. Aturan Dokumentasi Method

- Setiap public method memiliki ID dan entri.
- Parameter wajib dijelaskan (tipe, nullability, validasi).
- Return dan eksepsi dicantumkan.
- Efek samping ditandai jelas.
- Contoh pemanggilan untuk method kompleks.
