# 04 - Analisis Bottleneck

## 1. Daftar Bottleneck

| ID | Bottleneck | Lokasi | Bukti | Kompleksitas |
|---|---|---|---|---|
| B1 | Query stok per perubahan | `StockLedger.ApplyAsync` | 237 ms/100 changes | O(k) query |
| B2 | `SaveChanges` per nomor dokumen | `DocumentNumberService.NextAsync` | Dipanggil per movement | O(k) round-trip |
| B3 | N+1 creator approval | `ApprovalService.GetPendingAsync` | 15,3 ms/100 pending | O(n) query |
| B4 | Query stok per baris | StockOut/Transfer | 7,53 ms vs 0,64 ms batch | O(n) query |
| B5 | System quantity per baris | Adjustment/Opname | loop + query per detail | O(n) query |
| B6 | Materialisasi seluruh movement | `GetStockCardAsync` | 30,7 ms/5,22 MB | O(m) memori |
| B7 | Validasi principal per request | `OnValidatePrincipal` | 1 query/request | O(1) per request |
| B8 | Dua `SaveChanges` per save | `AppDbContext.SaveChangesAsync` | entity + audit | 2 round-trip |
| B9 | Sequence read-modify-write | `DocumentNumberService` | rawan konflik | O(1) + retry |

## 2. Bottleneck Utama

### B1/B2 — Ledger
- Setiap perubahan: SELECT stok + (jika error) SELECT produk/lokasi +
  `NextAsync` (yang menyimpan sequence) + insert movement.
- Dalam satu approval, perubahan bisa puluhan/ratusan.
- Dominasi: **I/O database**, bukan CPU.

### B3 — Approval
- 100 item = 1 query utama + 100 query creator + 1 query nama.
- N+1 klasik.

### B4/B5 — Validasi stok
- Satu query per baris. Untuk 100 baris = 100 round-trip.
- Batch `WHERE ProductId IN (...)` dapat menggantikan loop.

### B6 — Stock card
- Semua movement dimuat ke memori.
- 10.000 baris = 5,22 MB; 100.000 baris ≈ 52 MB (estimasi linear).

### B7 — Auth
- Setiap request (termasuk static? tidak) memvalidasi user ke DB.
- Beban naik linear dengan traffic.

## 3. Bottleneck Sekunder

- **B8**: audit ditulis pada `SaveChanges` kedua.
- **B9**: sequence read-modify-write berpotensi retry saat concurrent.
- **Report dashboard**: beberapa query agregasi terpisah (bukan N+1, tetapi
  bisa digabung).
- **CSV export**: dibatasi 200 baris, bukan bottleneck performa tetapi
  fungsional.

## 4. Prioritas Perbaikan

1. B1+B2 (ledger & nomor dokumen)
2. B3 (approval)
3. B4+B5 (stock check & system qty)
4. B6 (stock card pagination)
5. B7 (auth cache)
6. B8+B9 (audit & sequence)
