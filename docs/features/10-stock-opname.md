# Feature 10 - Stock Opname (Cycle Count)

- **ID:** F-10
- **Phase:** 3
- **Status:** Implemented
- **Depends on:** F-03, F-04, F-01, F-09

## 1. Ringkasan

Perhitungan fisik stok per lokasi untuk memverifikasi saldo sistem. Selisih
yang ditemukan ditindaklanjuti sebagai adjustment setelah approval.

## 2. Scope

**In scope**
- Buat sesi opname per lokasi, snapshot system qty.
- Input counted qty per produk.
- Submit & approval; selisih diterapkan ke stok.

**Out of scope**
- Freeze transaksi otomatis selama opname, barcode scanning.
- Opname terjadwal otomatis (scheduler).

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Staff | Buat & input hasil hitung |
| SPV | Approve/reject |
| Admin | Semua |

## 4. Data Model

**StockOpnameHeaders** (PK `Guid`, soft delete) + **StockOpnameDetails** (PK `Guid`, soft delete).

### 4.1 Field Dictionary - StockOpnameHeaders

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas sesi opname |
| `DocNo` | string(30) | Ya | Unik (aktif), auto | Nomor dokumen `OPN-yyyyMM-####` |
| `LocationId` | Guid | Ya | FK Locations, aktif | Lokasi yang diopname |
| `Date` | DateTime | Ya | - | Tanggal sesi |
| `Status` | enum | Ya | Default Draft | Status approval |
| `SubmittedAt` / `ApproverId` | DateTime? / int? | Tidak | - | Waktu submit & approver |
| `ApprovedAt` / `ApprovedById` | DateTime? / int? | Tidak | - | Waktu & user approve |
| `RejectionReason` | string? | Tidak | - | Alasan reject |
| `Notes` | string(500) | Tidak | - | Catatan sesi |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

### 4.2 Field Dictionary - StockOpnameDetails

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `Id` | Guid | Ya | PK | Identitas baris |
| `StockOpnameHeaderId` | Guid | Ya | FK header | Sesi induk |
| `ProductId` | Guid | Ya | FK Products, aktif | Produk yang dihitung |
| `SystemQuantity` | int | Ya | - | Snapshot stok sistem |
| `CountedQuantity` | int? | Tidak | `>= 0` | Hasil hitung fisik |
| `Difference` | int | Ya | - | `Counted - System` |
| `Notes` | string(500) | Tidak | - | Catatan baris |
| audit + soft delete | - | - | Otomatis | Jejak create/update/delete |

## 5. Workflow

1. Staff membuat sesi opname untuk satu lokasi.
2. Sistem snapshot `SystemQuantity` untuk semua stok pada lokasi terpilih; lokasi tanpa stok ditolak.
3. Staff input `CountedQuantity`.
4. Submit → SPV review selisih.
5. Approve → terapkan selisih ke `Stocks` + `StockMovements` Type=Opname.
6. Reject → alasan; Staff dapat koreksi hitung dan submit ulang.

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/stock-opnames` | List sesi |
| GET/POST | `/stock-opnames/create` | Buat sesi + pilih lokasi |
| GET/POST | `/stock-opnames/edit/{id}` | Input hasil hitung |
| POST | `/stock-opnames/submit/{id}` | Submit |
| POST | `/stock-opnames/approve/{id}` | Approve |
| POST | `/stock-opnames/reject/{id}` | Reject + reason |
| POST | `/stock-opnames/delete/{id}` | Soft delete (Draft) |
| GET | `/stock-opnames/details/{id}` | Detail + selisih |

## 7. Validasi & Business Rules

- Satu lokasi hanya boleh punya satu opname berstatus `Submitted` pada satu waktu.
- `CountedQuantity >= 0`.
- Snapshot system qty diambil saat sesi dibuat; di-refresh saat submit untuk akurasi.
- Approval menerapkan `CountedQuantity` sebagai saldo baru.

## 8. Edge Cases & Error Handling

- Transaksi lain mengubah stok selama opname → tandai selisih "stale", minta refresh sebelum approve.
- Produk hilang dari snapshot (stok 0) tetap tampil.
- Approve tanpa counted qty untuk suatu baris → baris dianggap belum dihitung & ditolak.

## 9. Security Considerations

- Hanya SPV/Admin yang approve.
- Audit lengkap nilai sistem vs hasil hitung.

## 10. Acceptance Criteria

- [ ] Sesi opname per lokasi bisa dibuat.
- [ ] System qty ter-snapshot.
- [ ] Selisih dihitung otomatis.
- [ ] Approve menerapkan saldo hasil hitung.
- [ ] Tidak bisa approve sesi milik sendiri.