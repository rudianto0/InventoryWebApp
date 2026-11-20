# 2. Buatlah Dokumentasi Modul Program

## a. Buat dokumentasi modul sesuai identitas dan jelaskan kegunaan modul

Setiap modul memiliki kartu identitas agar mudah dilacak.

| Field | Keterangan |
|---|---|
| Module ID | ID unik modul |
| Versi | Versi dokumen |
| Status | Draft, Review, Approved, Implemented |
| Owner | Penanggung jawab |
| File | Lokasi kode program |
| Dependensi | Modul lain yang dipakai |
| Dipakai oleh | Modul yang memanggil |
| Tanggung jawab | Tugas utama modul |
| Invarian | Aturan yang harus selalu benar |
| Eksepsi | Kesalahan yang mungkin muncul |
| Test | Pengujian terkait |
| Revisi | Tanggal perubahan |

**Contoh: `MOD-APP-LEDGER`**

| Field | Nilai |
|---|---|
| Module ID | `MOD-APP-LEDGER` |
| Versi | 1.0 |
| Status | Implemented |
| Owner | Tim Backend |
| File | `src/Inventory.Application/Services/StockLedger.cs` |
| Dependensi | `IAppDbContext`, `IDocumentNumberService` |
| Dipakai oleh | Stock In/Out/Transfer/Adjustment/Opname |
| Kegunaan | Mengubah saldo stok dan mencatat pergerakan |
| Invarian | Stok tidak boleh negatif; jumlah pergerakan tidak nol |
| Eksepsi | `ValidationException` |
| Test | `GuardAndLedgerTests` |
| Revisi | 2026-11-15 |

## b. Terapkan identifikasi dokumentasi sesuai identitas untuk memudahkan pelacakan

- Module ID memakai format `MOD-<LAYER>-<NAME>`, misalnya `MOD-APP-STOCKIN`.
- Method ID memakai format `MTH-<LAYER>-<MODULE>-<ACTION>`, misalnya
  `MTH-APP-STOCKIN-CREATE`.
- ID yang sama dipakai di dokumen modul, dokumen method, dan daftar pelacakan.

**Daftar pelacakan**

| Module ID | File | Test |
|---|---|---|
| `MOD-APP-LEDGER` | `StockLedger.cs` | `GuardAndLedgerTests` |
| `MOD-APP-STOCKIN` | `StockInService.cs` | `StockInServiceTests` |
| `MOD-APP-STOCKOUT` | `StockOutService.cs` | `StockOutServiceTests` |
| `MOD-APP-APPROVAL` | `ApprovalService.cs` | `ApprovalServiceTests` |
| `MOD-INF-DBCONTEXT` | `AppDbContext.cs` | `SoftDeleteTests` |
| `MOD-INF-DOCNUM` | `DocumentNumberService.cs` | `DocumentNumberServiceTests` |

Dengan cara ini, saat ada bug, pengembang dapat langsung menemukan file,
pengujian, dan dokumen yang terkait.

## c. Revisi dokumen sesuai perubahan kode program

Dokumen harus diperbarui setiap kali kode berubah.

| Perubahan kode | Dokumen yang diperbarui |
|---|---|
| Nama atau lokasi file | Kartu identitas modul |
| Parameter atau DTO | Bagian parameter |
| Alur algoritma | Bagian algoritma |
| Jenis eksepsi | Katalog eksepsi |
| Signature method | Dokumentasi method |

**Checklist teknis**

- [x] Modul memiliki ID unik.
- [x] Kegunaan modul dijelaskan.
- [x] Dependensi dan modul pemanggil dicatat.
- [x] Test terkait dicantumkan.
- [x] Tanggal revisi dicatat.
