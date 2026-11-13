# Feature 08 - Stock Transfer

- **ID:** F-08
- **Phase:** 3
- **Status:** Implemented
- **Depends on:** F-03, F-04, F-01, F-06, F-07

## 1. Ringkasan

Pemindahan stok antar lokasi (gudang/cabang) dengan approval. Menghasilkan
dua mutasi: keluar dari lokasi asal dan masuk ke lokasi tujuan.

## 2. Scope

**In scope**
- Draft transfer header + lines, submit, approve/reject.
- Mutasi `TransferOut` di asal dan `TransferIn` di tujuan saat Approved.
- Validasi stok asal mencukupi.

**Out of scope**
- Transfer bertahap (partial shipped/received), transit tracking.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Staff | Create/edit draft, submit |
| SPV | Approve/reject |
| Admin | Semua + override |

## 4. Data Model

**StockTransferHeaders**: Id, DocNo, FromLocationId, ToLocationId, Date, Status + field approval + CreatedById, CreatedAt, Notes.

**StockTransferDetails**: Id, StockTransferHeaderId, ProductId, Quantity (>0), Notes.

## 5. Workflow

1. Staff membuat draft: lokasi asal, lokasi tujuan (boleh lintas cabang).
2. Tambah baris produk & quantity.
3. Submit → `Submitted`, approver = parent SPV.
4. Approve → dalam 1 DB transaction:
   - `From.Quantity -= qty`, insert `StockMovements` Type=TransferOut (negatif)
   - `To.Quantity += qty`, insert `StockMovements` Type=TransferIn (positif)
5. Reject → alasan, submit ulang.

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/stock-transfers` | List + filter |
| GET/POST | `/stock-transfers/create` | Form draft |
| GET/POST | `/stock-transfers/edit/{id}` | Edit draft/rejected |
| POST | `/stock-transfers/submit/{id}` | Submit |
| POST | `/stock-transfers/approve/{id}` | Approve |
| POST | `/stock-transfers/reject/{id}` | Reject + reason |
| GET | `/stock-transfers/details/{id}` | Detail |

## 7. Validasi & Business Rules

- `FromLocationId != ToLocationId`.
- Kedua lokasi aktif.
- Stok asal cukup saat submit & approve.
- Quantity > 0.
- Tidak boleh approve transaksi sendiri.

## 8. Edge Cases & Error Handling

- Stok asal berubah antara submit & approve → cek ulang.
- Kegagalan salah satu sisi → rollback keduanya.
- Lokasi tujuan dinonaktifkan → tolak approve.

## 9. Security Considerations

- Otorisasi owner/approver.
- Mutasi dua sisi harus atomik.
- Audit status & mutasi.

## 10. Acceptance Criteria

- [ ] Draft transfer bisa dibuat & disubmit.
- [ ] Approve mengurangi stok asal & menambah stok tujuan secara atomik.
- [ ] Transfer ke lokasi yang sama ditolak.
- [ ] Stok asal tidak cukup ditolak.
