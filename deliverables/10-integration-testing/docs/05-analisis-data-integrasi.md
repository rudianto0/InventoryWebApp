# 05 - Analisis Data Pengujian Integrasi

## 1. Analisis Modul Terkait

| Alur | Modul | Hasil |
|---|---|---|
| Stock In → Approval → Ledger | StockInService, StockLedger, AppDbContext | Sesuai |
| Stock Out → Ledger | StockOutService, StockLedger | Sesuai |
| Transfer → dua saldo | StockTransferService, StockLedger | Sesuai |
| Adjustment → difference | StockAdjustmentService, StockLedger | Sesuai |
| Opname → counted | StockOpnameService, StockLedger | Sesuai |
| Approval | ApprovalGuard, IUserDirectory | Sesuai |
| Soft delete/audit | AppDbContext | Sesuai |

## 2. Perbandingan Data

| TestID | Expected | Actual | Status |
|---|---|---|---|
| INT-01 | stok 7, Approved | stok 7, Approved | Pass |
| INT-02 | stok 6, Out -4 | stok 6, Out -4 | Pass |
| INT-03 | error, stok 2 | error, stok 2 | Pass |
| INT-04 | WH1 6, WH2 4 | WH1 6, WH2 4 | Pass |
| INT-05 | diff -3, stok 7 | diff -3, stok 7 | Pass |
| INT-06 | stok 12 | stok 12 | Pass |
| INT-07 | error | error | Pass |
| INT-08 | Approved | Approved | Pass |
| INT-09 | hidden | hidden | Pass |
| INT-10 | audit 3 aksi | audit 3 aksi | Pass |

## 3. Ketidaksesuaian

Tidak ada ketidaksesuaian fungsional pada skenario INT-01–INT-10.

Kondisi yang belum sesuai cakupan:

| ID | Kondisi | Perkiraan |
|---|---|---|
| DEF-INT-01 | PostgreSQL nyata belum diuji | Perilaku constraint/locking berbeda |
| DEF-INT-02 | API/controller belum diuji | Error mapping/auth belum terverifikasi |
| DEF-INT-03 | UI belum diuji | Razor flow belum terverifikasi |

## 4. Evaluasi Standar

| Standar | Status |
|---|---|
| Expected vs actual tercatat | Ya |
| Isolasi test | Ya |
| Deterministik | Ya |
| Coverage dilaporkan | Ya |
| Regression | 171 pass |
| Integration PostgreSQL | Belum |

## 5. Kesimpulan Analisis

Integrasi antar modul inti berjalan sesuai expected pada level in-memory.
Risiko utama ada pada perbedaan provider database dan lapisan HTTP/UI.
