# 07 - Performance Profiling

## Konteks

Mengukur waktu eksekusi dan penggunaan memori komponen aplikasi Inventory,
mengidentifikasi bottleneck, merancang solusi, serta menganalisis kompleksitas
algoritma yang berdampak pada performa.

## Dokumen

| File | Isi |
|---|---|
| `docs/00-ringkasan.md` | Ringkasan hasil |
| `docs/01-metodologi-pengukuran.md` | Tools, environment, skenario |
| `docs/02-data-waktu-dan-memory.md` | Data waktu & memori |
| `docs/03-modul-bermasalah.md` | Modul terindikasi bermasalah |
| `docs/04-analisis-bottleneck.md` | Bottleneck |
| `docs/05-dampak-bottleneck.md` | Dampak negatif |
| `docs/06-rancangan-solusi.md` | Rancangan metode |
| `docs/07-peningkatan-performa.md` | Before/after performa |
| `docs/08-kompleksitas-algoritma.md` | Analisis kompleksitas |
| `docs/09-kesimpulan.md` | Kesimpulan |
| `Inventory-Performance-Analysis.docx` | Gabungan Word |

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
