# 09 - Software Testing

## Konteks

Dokumen pengujian aplikasi Inventory: kebutuhan uji, dokumentasi, data uji,
skenario, pelaksanaan, dan evaluasi hasil.

## Dokumen

| File | Isi |
|---|---|
| `docs/00-ringkasan.md` | Ringkasan hasil |
| `docs/01-kebutuhan-uji-coba.md` | SDLC, tools, standar |
| `docs/02-dokumentasi-uji-coba.md` | Kebutuhan dokumentasi & variasi |
| `docs/03-data-uji.md` | Data uji unit & generator |
| `docs/04-skenario-dan-prosedur.md` | Skenario + algoritma |
| `docs/05-pelaksanaan-uji-coba.md` | Eksekusi |
| `docs/06-hasil-uji-coba.md` | Hasil |
| `docs/07-analisis-hasil.md` | Analisis |
| `docs/08-laporan-dan-perbaikan.md` | Laporan & perbaikan |
| `Inventory-Software-Testing.docx` | Gabungan Word |

## Hasil Eksekusi

| Metrik | Nilai |
|---|---:|
| Total test | 171 |
| Passed | 171 |
| Failed | 0 |
| Skipped | 0 |
| Durasi | ±10 detik |
| Line coverage | 74.69% |
| Branch coverage | 60.17% |

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
