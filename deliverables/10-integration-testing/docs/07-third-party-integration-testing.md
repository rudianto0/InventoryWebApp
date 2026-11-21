# 7. Integration Testing — Procurement, Stock In, Approval, dan Third Party

Bagian ini menambahkan pengujian integrasi untuk alur **procurement → stock in →
approval → stock ledger**, termasuk simulasi integrasi dengan sistem third party.

## 7.1 Ruang Lingkup

Yang diuji:
- Payload procurement dari sistem luar dipetakan ke `StockInInput`.
- Stock In dibuat, disubmit, dan disetujui.
- Stok dan `StockMovements` diperbarui setelah approval.
- Approval oleh user berbeda (approver terpisah).
- Penolakan payload invalid.
- Audit trail create/submit/approve.
- Retry payload yang sama (mendokumentasikan gap idempotency).

Yang belum diuji:
- API vendor produksi nyata (yang diuji adalah HTTP stub lokal).
- API key/JWT produksi.
- PostgreSQL nyata.
- Layanan approval terpisah di luar aplikasi.

## 7.2 Arsitektur Test

Project baru:

```
tests/Inventory.IntegrationTests/
├─ IntegrationTestDatabase.cs
├─ ProcurementMapper.cs
├─ ProcurementToStockInIntegrationTests.cs
├─ ThirdPartyApprovalStub.cs
└─ ThirdPartyApprovalIntegrationTests.cs
```

Alur test procurement:

```
ProcurementOrder (payload simulasi)
   → ProcurementMapper
   → StockInInput
   → StockInService.CreateAsync
   → SubmitAsync
   → ApproveAsync / RejectAsync (approver berbeda)
   → Stocks + StockMovements + AuditLogs
```

Alur test approval third-party:

```
Test → HTTP POST /api/approvals (API key)
     → ThirdPartyApprovalStub (server HTTP lokal)
     → callback → StockInService.ApproveAsync/RejectAsync
     → Stocks + StockMovements + AuditLogs
```

Database: SQLite in-memory. Tidak ada perubahan kode produksi.

## 7.3 Data Uji

| Data | Nilai |
|---|---|
| Supplier | `SUP` |
| Lokasi | `WH1` |
| Produk | `SKU-1`, `SKU-2` |
| Produk nonaktif | `SKU-OFF` |
| Approver | user `20` |
| Staff | user `10` |
| Contoh PO | `PO-2026-001` |
| Quantity | 7 |
| Unit cost | 45.000 |

## 7.4 Skenario dan Hasil

Hasil dipisahkan menjadi dua tabel: integrasi internal (procurement/stock in/
approval) dan integrasi third party melalui HTTP. Hasil third party juga
dicatat pada sheet tersendiri `sheets/result-sheet-third-party.csv`.

### 7.4.1 Integrasi Internal (Procurement, Stock In, Approval)

| ID | Skenario | Test | Hasil |
|---|---|---|---|
| EXT-01 | Payload procurement valid membuat Draft | `ProcurementPayload_Valid_CreatesDraft` | Pass |
| EXT-02 | Submit Stock In dari payload procurement | `ProcurementPayload_SubmitAndApprove_UpdatesStockAndAudit` | Pass |
| EXT-03 | Approve oleh approver terpisah | `ProcurementPayload_SubmitAndApprove_UpdatesStockAndAudit` | Pass |
| EXT-04 | Supplier nonaktif ditolak | `ProcurementPayload_InactiveSupplier_IsRejected` | Pass |
| EXT-05 | Produk nonaktif ditolak | `ProcurementPayload_InactiveProduct_IsRejected` | Pass |
| EXT-06 | Multi-line dengan satu baris invalid | `ProcurementPayload_MultiLineOneInvalid_RejectsWholeDocument` | Pass |
| EXT-07 | Multi-line valid semua baris diterapkan | `ProcurementPayload_MultiLineAllValid_AppliesAllLines` | Pass |
| EXT-08 | External approval reject | `ExternalApproval_Reject_LeavesStockUnchanged` | Pass |
| EXT-09 | Approver tidak berwenang ditolak | `ExternalApproval_UnauthorizedApprover_IsRejected` | Pass |
| EXT-10 | Retry payload yang sama | `ProcurementPayload_Retry_CreatesSeparateDrafts_CurrentBehavior` | Pass (gap idempotency) |

### 7.4.2 Integrasi Third Party (HTTP Stub)

| ID | Endpoint | Auth | Skenario | Test | Status Code | Stock | Hasil |
|---|---|---|---|---|---|---|---|
| EXT-11 | `POST /api/approvals` | valid key | Approval third-party via HTTP | `ThirdPartyApproval_ApproveViaHttp_UpdatesStock` | 200 | 7 | Pass |
| EXT-12 | `POST /api/approvals` | valid key | Reject third-party via HTTP | `ThirdPartyApproval_RejectViaHttp_LeavesStockUnchanged` | 200 | 0 | Pass |
| EXT-13 | `POST /api/approvals` | invalid key | API key salah | `ThirdPartyApproval_InvalidApiKey_Returns401AndDoesNotApprove` | 401 | 0 | Pass |
| EXT-14 | `POST /api/approvals` | valid key | Third-party error 500 | `ThirdPartyApproval_ServerError_DoesNotApprove` | 500 | 0 | Pass |
| EXT-15 | `POST /api/approvals` | valid key | Callback approval duplikat | `ThirdPartyApproval_DuplicateCallback_DoesNotDoubleApply` | 200/409 | 7 | Pass |

Ringkasan:

```text
Project : Inventory.IntegrationTests
Total   : 14 test method (15 scenario ID)
Passed  : 14
Failed  : 0
Skipped : 0
Database: SQLite in-memory
Third party approval: HTTP stub lokal
Third party scenarios: 5 passed
```

## 7.5 Hasil yang Diverifikasi

- Payload procurement valid menghasilkan header/detail Stock In.
- Submit menetapkan status `Submitted` dan `ApproverId`.
- Approve menaikkan stok dan membuat `StockMovements` tipe `In`.
- Reject tidak mengubah stok.
- Approver yang tidak berwenang ditolak.
- Multi-line valid membuat movement untuk setiap baris.
- Satu baris invalid membatalkan seluruh dokumen.
- Audit log mencatat `Create`, `Submit`, dan `Approve`.

## 7.6 Gap Idempotency

Saat ini `StockInInput` dan `StockInHeader` belum memiliki `ExternalReference`.
Akibatnya, retry payload procurement yang sama akan membuat dokumen baru.

Test `ProcurementPayload_Retry_CreatesSeparateDrafts_CurrentBehavior`
mendokumentasikan perilaku ini. Untuk integrasi third-party produksi,
disarankan menambahkan:
- field `ExternalReference`,
- unique index `(SourceSystem, ExternalReference)`,
- pengecekan duplikat sebelum create.

## 7.7 Traceability

| Komponen | Lokasi |
|---|---|
| Test project | `tests/Inventory.IntegrationTests/` |
| Payload mapper | `tests/Inventory.IntegrationTests/ProcurementMapper.cs` |
| Test database | `tests/Inventory.IntegrationTests/IntegrationTestDatabase.cs` |
| Test skenario | `tests/Inventory.IntegrationTests/ProcurementToStockInIntegrationTests.cs` |
| Third-party approval stub | `tests/Inventory.IntegrationTests/ThirdPartyApprovalStub.cs` |
| Third-party approval tests | `tests/Inventory.IntegrationTests/ThirdPartyApprovalIntegrationTests.cs` |
| Hasil TRX | `results/integration/integration-test-results.trx` |
| Ringkasan | `results/integration/integration-test-summary.csv` |
| Coverage | `results/integration/coverage.cobertura.xml` |
| Lembar uji | `sheets/test-case-sheet.csv`, `sheets/result-sheet.csv` |
| Lembar hasil third party | `sheets/result-sheet-third-party.csv` |

## 7.8 Third-Party Approval API (HTTP Stub)

Untuk menguji approval yang dilakukan pihak ketiga, test menjalankan server HTTP
lokal (`ThirdPartyApprovalStub`) dan memanggilnya lewat `HttpClient`.

**Kontrak API:**

| Item | Nilai |
|---|---|
| Method | `POST` |
| Path | `/api/approvals` |
| Header | `X-Api-Key: test-key` |
| Body | `documentId`, `decision` (`approve`/`reject`), `reason` |
| Sukses | `200 OK` |
| API key salah | `401 Unauthorized` |
| Dokumen sudah diproses | `409 Conflict` |
| Error third-party | `500 Internal Server Error` |

**Yang diuji:**
- Approval via HTTP memicu `StockInService.ApproveAsync` melalui callback.
- Reject via HTTP memicu `StockInService.RejectAsync`.
- API key salah tidak memicu callback.
- Error 500 tidak mengubah status/stok.
- Callback duplikat tidak menggandakan stok.

Catatan: ini adalah **HTTP stub lokal**, bukan API vendor produksi. Tujuannya
menguji kontrak HTTP, API key, status code, dan alur approval, bukan koneksi ke
vendor eksternal.

## 7.9 Kesimpulan

Integrasi procurement → stock in → approval berhasil diuji pada level contract
dan service dengan 15 skenario lulus, termasuk 5 skenario approval third-party
melalui HTTP stub lokal. Pola ini membuktikan alur antar-modul dan kesiapan boundary
`Inventory.Client` untuk pemisahan API. Untuk integrasi third-party produksi,
langkah berikutnya adalah API intake, autentikasi, dan idempotency berbasis
`ExternalReference`.
