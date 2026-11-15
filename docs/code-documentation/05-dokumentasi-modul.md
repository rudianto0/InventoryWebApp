# 05 - Dokumentasi Modul

## 1. Format Identitas

Setiap modul didokumentasikan dengan format:

| Field | Keterangan |
|---|---|
| Module ID | `MOD-<LAYER>-<NAME>` |
| Versi | Versi dokumen |
| Status | Draft/Review/Approved/Implemented |
| Owner | Penanggung jawab |
| File | Lokasi source |
| Dependensi | Modul/abstraksi yang dipakai |
| Dipakai oleh | Modul pemanggil |
| Tanggung jawab | Fungsi utama |
| Invarian | Aturan yang dijaga |
| Eksepsi | Exception utama |
| Test | Test terkait |
| Revisi | Tanggal perubahan |

## 2. Ringkasan Modul

| Module ID | Kegunaan | Dependensi | Test |
|---|---|---|---|
| `MOD-APP-LEDGER` | Mutasi stok + movement | `IAppDbContext`, `IDocumentNumberService` | `GuardAndLedgerTests` |
| `MOD-APP-STOCKIN` | Penerimaan + approval | `StockLedger`, `IUserDirectory` | `StockInServiceTests` |
| `MOD-APP-STOCKOUT` | Pengeluaran + cek stok | `StockLedger` | `StockOutServiceTests` |
| `MOD-APP-TRANSFER` | Transfer antar lokasi | `StockLedger` | `StockTransferServiceTests` |
| `MOD-APP-ADJUSTMENT` | Koreksi stok | `StockLedger` | `StockAdjustmentServiceTests` |
| `MOD-APP-OPNAME` | Hitung fisik | `StockLedger` | `StockOpnameServiceTests` |
| `MOD-APP-APPROVAL` | Otorisasi approval | `IUserDirectory` | `ApprovalServiceTests` |
| `MOD-APP-REPORT` | Laporan | EF Core | `ReportServiceTests` |
| `MOD-APP-GUARD` | Validasi | - | `GuardValidationTests` |
| `MOD-INF-DBCONTEXT` | Audit/soft delete | EF Core, Identity | `SoftDeleteTests`, `AuditTrailExtraTests` |
| `MOD-INF-DOCNUM` | Nomor dokumen | EF Core | `DocumentNumberServiceTests` |
| `MOD-INF-USER` | User/role | Identity | `UserServiceTests` |
| `MOD-CLI-CLIENT` | Boundary client | Application | build |
| `MOD-WEB-UI` | Controller/views | Client | build |

## 3. Contoh Entri Lengkap

### MOD-APP-LEDGER

| Field | Nilai |
|---|---|
| Module ID | `MOD-APP-LEDGER` |
| Versi | 1.0 |
| Status | Implemented |
| Owner | Tim Backend |
| File | `src/Inventory.Application/Services/StockLedger.cs` |
| Dependensi | `IAppDbContext`, `IDocumentNumberService` |
| Dipakai oleh | Stock In/Out/Transfer/Adjustment/Opname |
| Tanggung jawab | Mengubah `Stocks` dan menulis `StockMovements` |
| Invarian | stok ≥ 0, quantity movement ≠ 0 |
| Eksepsi | `ValidationException` |
| Test | `GuardAndLedgerTests` |
| Revisi | 2026-11-15 |

## 4. Aturan Revisi

- Setiap perubahan method/parameter wajib memperbarui entri modul.
- Perubahan besar menaikkan versi minor.
- Status `Implemented` hanya jika build + test hijau.
