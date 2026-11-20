# 4. Menganalisis Data Pengujian Integrasi

## 4.1 Analisis modul yang terkait sesuai standar pengembangan

| Alur integrasi | Modul yang terlibat | Hasil |
|---|---|---|
| Barang masuk → persetujuan → stok | StockInService, StockLedger, AppDbContext | Sesuai |
| Barang keluar → pengurangan stok | StockOutService, StockLedger | Sesuai |
| Transfer → dua saldo | StockTransferService, StockLedger | Sesuai |
| Adjustment → selisih | StockAdjustmentService, StockLedger | Sesuai |
| Opname → hasil hitung | StockOpnameService, StockLedger | Sesuai |
| Persetujuan | ApprovalGuard, IUserDirectory | Sesuai |
| Hapus sementara dan audit | AppDbContext | Sesuai |

## 4.2 Evaluasi data keluaran dengan data yang direncanakan

| Test ID | Direncanakan | Hasil nyata | Status |
|---|---|---|---|
| INT-01 | Stok 7, Approved | Stok 7, Approved | Pass |
| INT-02 | Stok 6, keluar 4 | Stok 6, keluar 4 | Pass |
| INT-03 | Ditolak, stok 2 | Ditolak, stok 2 | Pass |
| INT-04 | WH1 6, WH2 4 | WH1 6, WH2 4 | Pass |
| INT-05 | Selisih -3, stok 7 | Selisih -3, stok 7 | Pass |
| INT-06 | Stok 12 | Stok 12 | Pass |
| INT-07 | Ditolak | Ditolak | Pass |
| INT-08 | Approved otomatis | Approved otomatis | Pass |
| INT-09 | Data tidak tampil | Data tidak tampil | Pass |
| INT-10 | Tiga catatan audit | Tiga catatan audit | Pass |

## 4.3 Catat status pada lembar pengujian

Status dicatat pada `sheets/result-sheet.csv` dengan kolom:

`TestID, Actual, Status, Evidence, Notes`

- **Actual** berisi hasil nyata.
- **Status** berisi Pass atau Fail.
- **Evidence** berisi nama test atau file TRX.
- **Notes** berisi kondisi khusus, misalnya "in-memory SQLite".

## 4.4 Catat kondisi data yang tidak sesuai dan perkiraannya

Tidak ada ketidaksesuaian fungsional pada INT-01 sampai INT-10. Kondisi yang
belum tercakup dicatat sebagai berikut:

| ID | Kondisi | Perkiraan |
|---|---|---|
| DEF-INT-01 | Database PostgreSQL nyata belum diuji | Aturan unik dan penguncian bisa berbeda |
| DEF-INT-02 | API/controller belum diuji | Pemetaan error dan hak akses belum terverifikasi |
| DEF-INT-03 | Tampilan web belum diuji | Alur halaman belum terverifikasi |

**Kesimpulan analisis:** integrasi antar modul inti berjalan sesuai rencana pada
level in-memory. Risiko utama ada pada perbedaan database dan lapisan web.
