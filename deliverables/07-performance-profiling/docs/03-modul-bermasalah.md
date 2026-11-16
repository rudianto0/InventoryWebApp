# 03 - Modul Terindikasi Bermasalah

## 1. Daftar Modul

| Module ID | Modul | Indikasi | Prioritas |
|---|---|---|---|
| `MOD-APP-LEDGER` | StockLedger | Query stok per change + `SaveChanges` per nomor | Tinggi |
| `MOD-INF-DOCNUM` | DocumentNumberService | Read-modify-write + `SaveChanges` tiap nomor | Tinggi |
| `MOD-APP-APPROVAL` | ApprovalService | N+1 creator lookup | Tinggi |
| `MOD-APP-STOCKOUT` | StockOutService | Query stok per baris | Tinggi |
| `MOD-APP-TRANSFER` | StockTransferService | Query stok per baris | Tinggi |
| `MOD-APP-ADJUSTMENT` | StockAdjustmentService | Query system qty per baris | Sedang |
| `MOD-APP-OPNAME` | StockOpnameService | Refresh system qty per baris | Tinggi |
| `MOD-APP-REPORT` | ReportService | Stock card tanpa pagination | Tinggi |
| `MOD-INF-DBCONTEXT` | AppDbContext | Dua `SaveChanges` (entity + audit) | Sedang |
| `MOD-WEB-UI` | Auth/cookie | Validasi principal per request | Sedang |

## 2. Bukti Kode

### Ledger
- `foreach (var change in changes)` → `db.Stocks.FirstOrDefaultAsync`.
- `documents.NextAsync` dipanggil per movement; implementasi aslinya
  `SaveChangesAsync` per nomor.
- Benchmark: 237 ms / 40 MB untuk 100 changes.

### Approval
- Loop `foreach (var item in items)` → query `CreatedById` per item.
- Benchmark: 15,3 ms untuk 100 pending, ±105 query.

### Stock Out / Transfer
- `foreach (var line in stockLines)` → query `Stocks` per baris.
- Benchmark stock check: 7,53 ms vs 0,64 ms batch.

### Opname / Adjustment
- Loop detail → `GetSystemQuantityAsync` per baris.

### Report
- `GetStockCardAsync` mengembalikan seluruh movement.
- Benchmark: 30,7 ms / 5,22 MB untuk 10.000 movement.

### Auth
- `OnValidatePrincipal` memanggil `UserManager.GetUserAsync` setiap request.

## 3. Modul yang Tidak Bermasalah Signifikan

| Modul | Alasan |
|---|---|
| `MOD-APP-CATALOG` | CRUD sederhana + pagination |
| `MOD-APP-SUPPLIER` | CRUD sederhana |
| `MOD-APP-LOCATION` | CRUD + validasi hierarki |
| `MOD-APP-GUARD` | O(1) validasi |
| `MOD-APP-AUDIT` | Query dengan pagination |
| `MOD-INF-USER` | Operasi user jarang |

## 4. Kesimpulan

Modul bermasalah terkonsentrasi pada **alur transaksi stok** dan **pembacaan
histori**, bukan pada CRUD master atau validasi.
