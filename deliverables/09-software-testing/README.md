# 09 - Software Testing

## Konteks

Dokumen pengujian aplikasi Inventory: kebutuhan uji, dokumentasi, data uji,
pelaksanaan, dan evaluasi hasil.

## Dokumen

`docs/` disusun mengikuti 5 langkah kerja:

| File | Butir |
|---|---|
| `docs/01-kebutuhan-uji-coba.md` | 1. Kebutuhan uji coba (SDLC, tools, standar) |
| `docs/02-dokumentasi-uji-coba.md` | 2. Dokumentasi, variasi, skenario |
| `docs/03-data-uji.md` | 3. Data uji dan cara membangkitkannya |
| `docs/04-pelaksanaan-uji-coba.md` | 4. Skenario, algoritma, pelaksanaan |
| `docs/05-evaluasi-hasil.md` | 5. Hasil, analisis, laporan, perbaikan |
| `09-Inventory-Software-Testing.docx` | Gabungan Word |

## Hasil Eksekusi

| Metrik | Nilai |
|---|---:|
| Total test | 171 |
| Passed | 171 |
| Failed | 0 |
| Skipped | 0 |
| Durasi | ±10 detik |
| Line coverage | 74,69% |
| Branch coverage | 60,17% |

## Artefak

- `results/test-results.trx`
- `results/test-results.csv`
- `results/test-summary.csv`
- `results/coverage.cobertura.xml`
- `results/coverage-summary.txt`
- `assets/test-summary.png`

## Menjalankan

```powershell
dotnet test tests/Inventory.UnitTests/Inventory.UnitTests.csproj `
  --logger "trx;LogFileName=test-results.trx" `
  --collect:"XPlat Code Coverage"
```
