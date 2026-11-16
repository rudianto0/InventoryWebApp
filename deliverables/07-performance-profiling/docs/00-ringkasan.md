# 00 - Ringkasan

## Ringkasan Eksekusi

Pengukuran dilakukan dengan **BenchmarkDotNet 0.14.0** pada .NET 10.0.15,
menggunakan SQLite in-memory (warmup 1, iterasi 3). Data lengkap ada di
`measurements/`.

| Komponen | Skenario | Mean | Alokasi | Catatan |
|---|---|---:|---:|---|
| `StockLedger.ApplyAsync` | 100 changes | **237.4 ms** | **40.3 MB** | Query per change |
| `ApprovalService.GetPendingAsync` | 100 pending | **15.3 ms** | **1.28 MB** | N+1 creator lookup |
| `ReportService.GetStockCardAsync` | 10.000 movement | **30.7 ms** | **5.22 MB** | Tanpa pagination |
| Stock check per baris | 100 produk | **7.53 ms** | **1.16 MB** | 100 query |
| Stock check batch | 100 produk | **0.64 ms** | **173 KB** | 1 query |
| Creator lookup linear | 100 header | **3.89 µs** | **8.59 KB** | O(n) |
| Creator lookup dictionary | 100 header | **1.27 µs** | **3.05 KB** | O(1) |

## Temuan Utama

1. **Ledger** mendominasi waktu & memori: 237 ms dan 40 MB untuk 100 perubahan.
2. **Stock check per baris** 11,8× lebih lambat dan 6,7× lebih boros memori
   dibanding satu query batch.
3. **Approval** melakukan query creator per item (N+1).
4. **Stock card** memuat seluruh movement sekaligus; memori naik linear.
5. **Document number** melakukan `SaveChanges` per nomor.
6. **Auth** memvalidasi principal ke database setiap request.

## Rekomendasi

| Prioritas | Solusi |
|---|---|
| Tinggi | Batch stock query & batch document number |
| Tinggi | Hilangkan N+1 approval |
| Tinggi | Pagination stock card |
| Sedang | Batch system quantity pada adjustment/opname |
| Sedang | Cache validasi user & opsi master |
| Sedang | Kurangi `SaveChanges` audit |
| Rendah | Rate limit/compression |

## Kesimpulan

Bottleneck utama bukan CPU, melainkan **jumlah round-trip database** dan
**materialisasi data besar**. Perbaikan algoritma akses data (batch, pagination,
cache) diprediksi memberi peningkatan signifikan tanpa perlu mengganti arsitektur.
