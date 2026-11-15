# Feature 09 - Stock Adjustment

- **ID:** F-09
- **Phase:** 3
- **Status:** Implemented
- **Depends on:** F-03, F-04, F-01

## 1. Ringkasan

Penyesuaian saldo stok karena selisih (rusak, hilang, koreksi) dengan alasan
dan approval. Mencatat system quantity vs counted quantity.

## 2. Scope

**In scope**
- Draft adjustment + lines (system qty otomatis, counted qty input).
- Hitung `Difference = Counted - System`.
- Approval, lalu update stok & `StockMovements` Type=Adjustment.

**Out of scope**
- Opname massal (lihat F-10), koreksi biaya/valuasi.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Staff | Create/edit draft, submit |
| SPV | Approve/reject |
| Admin | Semua |

## 4. Data Model

**StockAdjustmentHeaders** (PK `Guid`, soft delete) + **StockAdjustmentDetails** (PK `Guid`, soft delete).

### 4.1 Field Dictionary - StockAdjustmentHeaders

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas dokumen adjustment |
| `DocNo` | string(30) | Ya | Unik (aktif), auto | Nomor dokumen `ADJ-yyyyMM-####` |
| `LocationId` | Guid | Ya | FK Locations, aktif | Lokasi yang disesuaikan |
| `Date` | DateTime | Ya | - | Tanggal adjustment |
| `Reason` | string(300) | Ya | - | Alasan koreksi |
| `Status` | enum | Ya | Default Draft | Status approval |
| `SubmittedAt` / `ApproverId` | DateTime? / int? | Tidak | - | Waktu submit & approver |
| `ApprovedAt` / `ApprovedById` | DateTime? / int? | Tidak | - | Waktu & user approve |
| `RejectionReason` | string? | Tidak | - | Alasan reject |
| `Notes` | string(500) | Tidak | - | Catatan dokumen |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

### 4.2 Field Dictionary - StockAdjustmentDetails

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas baris |
| `StockAdjustmentHeaderId` | Guid | Ya | FK header | Dokumen induk |
| `ProductId` | Guid | Ya | FK Products, aktif | Produk yang dikoreksi |
| `SystemQuantity` | int | Ya | - | Stok sistem saat snapshot |
| `CountedQuantity` | int | Ya | `>= 0` | Hasil hitung/koreksi |
| `Difference` | int | Ya | - | `Counted - System` |
| `Notes` | string(500) | Tidak | - | Catatan baris |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

## 5. Workflow

1. Staff pilih lokasi, tambah produk.
2. Sistem mengisi `SystemQuantity` dari `Stocks`.
3. Staff input `CountedQuantity`; `Difference` dihitung otomatis.
4. Submit → approval.
5. Approve → set stok menjadi `CountedQuantity` (atau delta = Difference),
   insert `StockMovements` (Quantity = Difference, bisa + / -).
6. Reject → alasan wajib.

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/stock-adjustments` | List + filter |
| GET/POST | `/stock-adjustments/create` | Form draft |
| GET/POST | `/stock-adjustments/edit/{id}` | Edit draft/rejected |
| POST | `/stock-adjustments/submit/{id}` | Submit |
| POST | `/stock-adjustments/approve/{id}` | Approve |
| POST | `/stock-adjustments/reject/{id}` | Reject + reason |
| POST | `/stock-adjustments/delete/{id}` | Soft delete (Draft) |
| GET | `/stock-adjustments/details/{id}` | Detail |

## 7. Validasi & Business Rules

- `Reason` wajib.
- `CountedQuantity >= 0`.
- Minimal 1 baris.
- Difference = 0 boleh disimpan tapi tidak menghasilkan movement.
- Stok akhir tidak boleh negatif.

## 8. Edge Cases & Error Handling

- Stok berubah setelah draft dibuat → `SystemQuantity` di-refresh saat submit/approve.
- Semua baris Difference 0 → approve tanpa mutasi (tetap tercatat dokumen).
- Concurrent update stok → row lock.

## 9. Security Considerations

- Adjustment adalah operasi sensitif → hanya role berwenang + approval.
- Audit wajib mencatat nilai lama/baru.

## 10. Acceptance Criteria

- [ ] Difference dihitung otomatis.
- [ ] Approve menyesuaikan stok sesuai counted qty.
- [ ] Reason wajib.
- [ ] Movement tercatat dengan delta yang benar.