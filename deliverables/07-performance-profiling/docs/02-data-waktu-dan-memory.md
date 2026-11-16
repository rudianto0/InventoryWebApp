# 02 - Data Waktu Eksekusi dan Memory

Sumber: `measurements/baseline.csv`, `measurements/memory.csv`.

## 1. Waktu Eksekusi

| Benchmark | Skenario | Mean | StdDev | Query DB (estimasi) |
|---|---|---:|---:|---:|
| `Ledger_Apply_100` | 100 changes | **237,416 µs (237.4 ms)** | 103,042 µs | ~100 select + `SaveChanges` |
| `Approval_Pending_100` | 100 pending | **15,285.9 µs (15.3 ms)** | 1,266 µs | ~105 query |
| `StockCard_10000` | 10.000 movement | **30,714.5 µs (30.7 ms)** | 531 µs | 1 query + materialisasi |
| `StockCheck_Naive_100` | 100 produk | **7,525.2 µs (7.53 ms)** | 1,491 µs | 100 query |
| `StockCheck_Batch_100` | 100 produk | **637.0 µs (0.64 ms)** | 91 µs | 1 query |
| `CreatorLookup_Naive_100` | 100 header | **3.887 µs** | 0.137 µs | 0 query (memory) |
| `CreatorLookup_Batch_100` | 100 header | **1.266 µs** | 0.045 µs | 0 query (memory) |

## 2. Penggunaan Memory

| Benchmark | Allocated | Gen0/1000 | Gen1/1000 | Gen2/1000 |
|---|---:|---:|---:|---:|
| `Ledger_Apply_100` | **40,348.92 KB** | 17,000 | 1,000 | 0 |
| `Approval_Pending_100` | **1,278.94 KB** | 625 | 0 | 0 |
| `StockCard_10000` | **5,221.27 KB** | 906.25 | 437.5 | 93.75 |
| `StockCheck_Naive_100` | **1,158.14 KB** | 562.5 | 0 | 0 |
| `StockCheck_Batch_100` | **172.78 KB** | 83.98 | 0 | 0 |
| `CreatorLookup_Naive_100` | **8.59 KB** | 4.21 | 0 | 0 |
| `CreatorLookup_Batch_100` | **3.05 KB** | 1.49 | 0 | 0 |

## 3. Analisis Singkat

### Ledger
- 237 ms untuk 100 changes ≈ **2,37 ms/change**.
- 40 MB alokasi menunjukkan entity tracking + movement besar.
- Setiap change melakukan query stok; dokumen number juga memicu `SaveChanges`.

### Approval
- 15,3 ms untuk 100 pending.
- Estimasi 105 query → **N+1**.
- Alokasi 1,28 MB masih wajar, tetapi waktu naik linear dengan pending.

### Stock Card
- 30,7 ms dan 5,22 MB untuk 10.000 movement.
- Ada **Gen2** (93,75/1000) → objek besar/berumur panjang.
- Tanpa pagination, memori tumbuh linear.

### Stock Check
- Naive 100 query: 7,53 ms, 1,16 MB.
- Batch 1 query: 0,64 ms, 173 KB.
- **11,8× lebih cepat; 6,7× lebih hemat memori.**

### Creator Lookup
- Linear scan: 3,89 µs.
- Dictionary: 1,27 µs.
- **3,1× lebih cepat; 2,8× lebih hemat memori.**

## 4. Catatan Interpretasi

- Query count adalah estimasi dari struktur kode; aktual dapat diverifikasi
  dengan EF logging.
- SQLite in-memory mengurangi biaya I/O; pada PostgreSQL selisih query
  per-baris vs batch biasanya **lebih besar**.
- Benchmark singkat (3 iterasi) cocok untuk indikasi awal, bukan sertifikasi.
