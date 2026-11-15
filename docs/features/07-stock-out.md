# Feature 07 - Stock Out (Issuing)

- **ID:** F-07
- **Phase:** 2
- **Status:** Implemented
- **Depends on:** F-03, F-04, F-01, F-06

## 1. Ringkasan

Pencatatan barang keluar dari lokasi (pemakaian/permintaan) dengan approval
single-level. Stok berkurang saat Approved.

## 2. Scope

**In scope**
- Draft Stock Out + lines, submit, approve/reject.
- Validasi ketersediaan stok per produk/lokasi.
- Update `Stocks` + `StockMovements` saat Approved.

**Out of scope**
- Sales order, pengiriman customer, retur.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Staff | Create/edit draft, submit |
| SPV | Approve/reject |
| Admin | Semua + override |

## 4. Data Model

**StockOutHeaders** (PK `Guid`, soft delete) + **StockOutDetails** (PK `Guid`, soft delete).

### 4.1 Field Dictionary - StockOutHeaders

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas dokumen Stock Out |
| `DocNo` | string(30) | Ya | Unik (aktif), auto | Nomor dokumen `OUT-yyyyMM-####` |
| `LocationId` | Guid | Ya | FK Locations, aktif | Lokasi sumber barang |
| `Date` | DateTime | Ya | - | Tanggal transaksi |
| `Purpose` | string(200) | Tidak | - | Tujuan pemakaian |
| `Status` | enum | Ya | Default Draft | Status approval |
| `SubmittedAt` / `ApproverId` | DateTime? / int? | Tidak | - | Waktu submit & approver |
| `ApprovedAt` / `ApprovedById` | DateTime? / int? | Tidak | - | Waktu & user approve |
| `RejectionReason` | string? | Tidak | - | Alasan reject |
| `Notes` | string(500) | Tidak | - | Catatan dokumen |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

### 4.2 Field Dictionary - StockOutDetails

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas baris |
| `StockOutHeaderId` | Guid | Ya | FK StockOutHeaders | Dokumen induk |
| `ProductId` | Guid | Ya | FK Products, aktif | Produk yang keluar |
| `Quantity` | int | Ya | `> 0` | Jumlah keluar |
| `Notes` | string(500) | Tidak | - | Catatan baris |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

## 5. Workflow

1. Staff membuat draft, pilih lokasi sumber, tambah baris & purpose.
2. Submit → `Status = Submitted`, `ApproverId = CurrentUser.ParentId`.
3. SPV approve/reject.
4. Approve → dalam 1 DB transaction:
   - validasi & kurangi `Stocks.Quantity`
   - insert `StockMovements` (Type = Out, Quantity negatif)
5. Reject → alasan wajib, bisa submit ulang.

## 6. Routing / Contract

| Method | Path | Deskripsi | Akses |
|--------|------|-----------|-------|
| GET | `/stock-outs` | List + filter | Auth |
| GET/POST | `/stock-outs/create` | Form draft | Staff/Admin |
| GET/POST | `/stock-outs/edit/{id}` | Edit draft/rejected | Owner/Admin |
| POST | `/stock-outs/submit/{id}` | Submit | Owner |
| POST | `/stock-outs/approve/{id}` | Approve | SPV/Admin |
| POST | `/stock-outs/reject/{id}` | Reject + reason | SPV/Admin |
| POST | `/stock-outs/delete/{id}` | Soft delete (Draft) | Owner/Admin |
| GET | `/stock-outs/details/{id}` | Detail | Auth |

## 7. Validasi & Business Rules

- Minimal 1 baris, quantity > 0, produk & lokasi aktif.
- Cek stok tersedia: `Stock.Quantity >= requested` saat submit **dan** approve.
- Tidak boleh approve transaksi sendiri.
- Approver harus aktif & ter-assign.

## 8. Edge Cases & Error Handling

- Stok berubah antara submit & approve → cek ulang saat approve; bila tidak cukup, tolak dengan pesan.
- Stok tidak boleh negatif (constraint `Quantity >= 0`).
- Concurrent approve → row lock / concurrency token.

## 9. Security Considerations

- Otorisasi owner/approver, cegah IDOR.
- Efek stok server-side.
- Audit setiap perubahan status.

## 10. Acceptance Criteria

- [ ] Draft bisa dibuat & disubmit.
- [ ] Stok tidak cukup ditolak saat submit dan approve.
- [ ] Approve mengurangi stok + membuat StockMovement.
- [ ] Reject wajib alasan.
- [ ] Tidak bisa approve transaksi sendiri.