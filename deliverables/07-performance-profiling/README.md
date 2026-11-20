# 07 - Performance Profiling

## Konteks

Mengukur waktu eksekusi dan penggunaan memori komponen aplikasi Inventory,
mengidentifikasi bottleneck, merancang solusi, serta menganalisis kompleksitas
algoritma yang berdampak pada performa.

## Dokumen

`docs/` disusun mengikuti 4 langkah kerja:

| File | Butir |
|---|---|
| `docs/01-data-waktu-dan-memory.md` | 1. Data waktu eksekusi dan memori |
| `docs/02-bottleneck-performa.md` | 2. Bottleneck dan dampaknya |
| `docs/03-rancangan-solusi.md` | 3. Rancangan solusi dan peningkatan |
| `docs/04-kompleksitas-algoritma.md` | 4. Kompleksitas algoritma |
| `07-Inventory-Performance-Analysis.docx` | Gabungan Word |

## Data

- `measurements/baseline.csv`
- `measurements/after-optimization.csv`
- `measurements/memory.csv`
- `assets/performance-chart.png`

## Benchmark

Project: `tests/Inventory.Benchmarks` (BenchmarkDotNet 0.14.0).

```powershell
dotnet run -c Release --project tests/Inventory.Benchmarks -- --filter "*"
```

## Catatan

Pengukuran baseline memakai **SQLite in-memory** agar reproducible. Angka absolut
tidak identik dengan PostgreSQL produksi, tetapi polanya (jumlah query, alokasi,
kompleksitas) tetap relevan. Validasi akhir perlu PostgreSQL + data volumetrik.
