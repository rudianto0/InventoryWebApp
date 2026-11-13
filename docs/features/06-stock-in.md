# Feature 06 - Stock In (Receiving)

- **ID:** F-06
- **Phase:** 2
- **Status:** Implemented
- **Depends on:** F-03, F-04, F-05, F-01

## 1. Ringkasan

Pencatatan barang masuk dari supplier ke lokasi. Mendukung draft,
submit, dan approval single-level ke SPV. Stok bertambah hanya saat Approved.

## 2. Scope

**In scope**
- Create/edit draft Stock In (header + lines).
- Submit ke SPV, approve/reject.
- Update `Stocks` + tulis `StockMovements` saat Approved.
- Nomor dokumen otomatis `IN-yyyyMM-####`.

**Out of scope**
- Purchase Order, penerimaan partial per PO, retur ke supplier (future).

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Staff | Create/edit draft, submit |
| SPV | Approve/reject milik bawahan |
| Admin | Semua + override |

## 4. Data Model

**StockInHeaders**: Id, DocNo, SupplierId, LocationId, Date, Status,
SubmittedAt, ApproverId, ApprovedAt, ApprovedById, RejectionReason, CreatedById, CreatedAt, Notes.

**StockInDetails**: Id, StockInHeaderId, ProductId, Quantity (>0), UnitCost?, Notes.

## 5. Workflow

1. Staff membuat draft, pilih supplier & lokasi, tambah baris produk.
2. Simpan sebagai Draft (`StockInHeaders.Status = Draft`).
3. Submit → `Status = Submitted`, `ApproverId = CurrentUser.ParentId`.
4. SPV membuka `/approvals`, approve/reject.
5. Reject → `Status = Rejected`, wajib `RejectionReason`; Staff perbaiki & submit ulang.
6. Approve → `Status = Approved`; dalam 1 DB transaction:
   - upsert `Stocks.Quantity += detail.Quantity`
   - insert `StockMovements` (Type = In, Quantity positif)
7. Semua aksi tercatat di AuditLog.

## 6. Routing / Contract

| Method | Path | Deskripsi | Akses |
|--------|------|-----------|-------|
| GET | `/stock-ins` | List + filter status/date | Auth |
| GET/POST | `/stock-ins/create` | Form draft | Staff/Admin |
| GET/POST | `/stock-ins/edit/{id}` | Edit draft/rejected | Owner/Admin |
| POST | `/stock-ins/submit/{id}` | Submit | Owner |
| POST | `/stock-ins/approve/{id}` | Approve | SPV/Admin |
| POST | `/stock-ins/reject/{id}` | Reject + reason | SPV/Admin |
| GET | `/stock-ins/details/{id}` | Detail | Auth |

### Detail line (dynamic rows)
```csharp
public sealed class StockInLineViewModel
{
    public int ProductId { get; set; }
    [Range(0.0001, double.MaxValue)] public decimal Quantity { get; set; }
    [Range(0, double.MaxValue)] public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
}
```

## 7. Validasi & Business Rules

- Minimal 1 baris detail; produk aktif; quantity > 0.
- Supplier & lokasi wajib, harus aktif.
- Hanya pemilik draft/rejected yang boleh edit & submit.
- Tidak boleh approve transaksi milik sendiri.
- Approver harus aktif; jika `ParentId` null → submit ditolak.
- Approve idempoten: transaksi berstatus Approved tidak bisa diproses ulang.

## 8. Edge Cases & Error Handling

- Produk dinonaktifkan setelah draft dibuat → tolak submit, minta ganti.
- Concurrent approve → gunakan optimistic concurrency/row lock, proses kedua ditolak.
- Gagal simpan stok → rollback seluruh transaksi.

## 9. Security Considerations

- Otorisasi per-owner & per-approver (bukan hanya role).
- Anti-forgery token; validasi IDOR pada setiap akses header/detail.
- Efek stok hanya di server, bukan dari input client.

## 10. Acceptance Criteria

- [ ] Draft bisa dibuat/diedit/dihapus (saat masih Draft).
- [ ] Submit menetapkan approver = parent SPV.
- [ ] Approve menambah stok + membuat StockMovement.
- [ ] Reject wajib alasan dan bisa di-submit ulang.
- [ ] Tidak bisa approve transaksi sendiri.
- [ ] Nomor dokumen unik berurutan.
