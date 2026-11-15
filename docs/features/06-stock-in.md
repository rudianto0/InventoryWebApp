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

**StockInHeaders** (PK `Guid`, soft delete) + **StockInDetails** (PK `Guid`, soft delete).

### 4.1 Field Dictionary - StockInHeaders

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas dokumen Stock In |
| `DocNo` | string(30) | Ya | Unik (aktif), auto | Nomor dokumen `IN-yyyyMM-####` |
| `SupplierId` | Guid | Ya | FK Suppliers, aktif | Supplier asal barang |
| `LocationId` | Guid | Ya | FK Locations, aktif | Lokasi penerima |
| `Date` | DateTime | Ya | - | Tanggal transaksi |
| `Status` | enum | Ya | Default Draft | Status approval |
| `SubmittedAt` | DateTime? | Tidak | - | Waktu submit |
| `ApproverId` | int? | Tidak | `Users.Id` | SPV yang ditunjuk |
| `ApprovedAt` | DateTime? | Tidak | - | Waktu approve |
| `ApprovedById` | int? | Tidak | `Users.Id` | User yang approve |
| `RejectionReason` | string? | Tidak | - | Alasan reject |
| `Notes` | string(500) | Tidak | - | Catatan dokumen |
| `CreatedAt` / `CreatedById` | DateTime / int? | Ya / Tidak | Otomatis | Audit pembuatan |
| `UpdatedAt` / `UpdatedById` | DateTime? / int? | Tidak | Otomatis | Audit perubahan |
| `IsDeleted` / `DeletedAt` / `DeletedById` | bool / DateTime? / int? | Ya / Tidak / Tidak | Otomatis | Soft delete & audit hapus |

### 4.2 Field Dictionary - StockInDetails

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas baris |
| `StockInHeaderId` | Guid | Ya | FK StockInHeaders | Dokumen induk |
| `ProductId` | Guid | Ya | FK Products, aktif | Produk yang diterima |
| `Quantity` | int | Ya | `> 0` | Jumlah diterima |
| `UnitCost` | decimal(18,4)? | Tidak | `>= 0` | Harga satuan (valuasi) |
| `Notes` | string(500) | Tidak | - | Catatan baris |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

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
| POST | `/stock-ins/delete/{id}` | Soft delete (Draft) | Owner/Admin |
| GET | `/stock-ins/details/{id}` | Detail | Auth |

### Detail line (dynamic rows)
```csharp
public sealed class StockInLineViewModel
{
    public Guid ProductId { get; set; }
    [Range(1, int.MaxValue)] public int Quantity { get; set; }
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