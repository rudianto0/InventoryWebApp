# 01 - Metodologi Pengukuran

## 1. Tujuan

Mengumpulkan data waktu eksekusi dan memori untuk function/procedure/method,
mengidentifikasi modul bermasalah, lalu merancang dan mengukur perbaikan.

## 2. Tools

| Tool | Fungsi |
|---|---|
| BenchmarkDotNet 0.14.0 | Mean, error, alokasi, GC |
| .NET SDK 10.0.404 | Runtime & build |
| SQLite in-memory | Database uji reproducible |
| EF Core 10 | Query & tracking |
| `dotnet-counters` | CPU/GC/working set |
| `dotnet-trace` | Trace eksekusi |
| `pg_stat_statements` | Query PostgreSQL paling mahal |
| `EXPLAIN (ANALYZE, BUFFERS)` | Rencana eksekusi |

## 3. Environment

| Aspek | Nilai |
|---|---|
| OS | Windows 11 |
| Runtime | .NET 10.0.15 |
| CPU | X64 (BenchmarkDotNet host) |
| GC | Concurrent Workstation |
| Database | SQLite in-memory |
| Konfigurasi | Release |
| Warmup | 1 |
| Iteration | 3 |

## 4. Skenario

| ID | Benchmark | Variabel |
|---|---|---|
| PERF-01 | `Ledger_Apply_100` | 100 stock changes |
| PERF-02 | `Approval_Pending_100` | 100 pending approvals |
| PERF-03 | `StockCard_10000` | 10.000 movement |
| PERF-04 | `StockCheck_Naive_100` | 100 query per baris |
| PERF-05 | `StockCheck_Batch_100` | 100 produk satu query |
| PERF-06 | `CreatorLookup_Naive_100` | linear scan 100 |
| PERF-07 | `CreatorLookup_Batch_100` | dictionary 100 |

## 5. Metrik

- **Mean** waktu eksekusi (ms/µs).
- **Allocated** memori per operasi (KB).
- **Gen0/Gen1/Gen2** per 1000 operasi.
- Jumlah query DB (dari kode/EF logging).
- Kompleksitas algoritma (O).

## 6. Perintah

```powershell
dotnet run -c Release --project tests/Inventory.Benchmarks -- --filter "*"
```

Hasil detail ada di `tests/Inventory.Benchmarks/BenchmarkDotNet.Artifacts/` dan
ringkasan disalin ke `measurements/`.

## 7. Batasan

- SQLite in-memory lebih cepat dari PostgreSQL untuk query sederhana dan tidak
  memiliki lock/IO yang sama.
- Angka absolut bukan patokan produksi; yang dibandingkan adalah **pola** dan
  **rasio** (per-line vs batch, materialisasi vs pagination).
- Belum ada load test HTTP; pengukuran fokus pada layer Application/EF.
- Dataset 100 produk/10.000 movement adalah skala kecil–menengah.

## 8. Validasi Lanjutan

1. Jalankan skrip `scripts/postgresql-profiling.sql`.
2. Ulangi benchmark dengan PostgreSQL dan 100k movement.
3. Jalankan k6/NBomber untuk endpoint.
4. Bandingkan p50/p95, query count, dan memori.
