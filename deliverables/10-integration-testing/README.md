# 10 - Integration Testing Documentation

## Konteks

Dokumentasi peralatan, lingkungan, data uji integrasi, pelaksanaan uji modul,
analisis hasil, dan arsip pengujian.

## Dokumen

| File | Isi |
|---|---|
| `docs/00-ringkasan.md` | Ringkasan hasil |
| `docs/01-peralatan-dan-lingkungan.md` | Peralatan & lingkungan |
| `docs/02-dokumen-pendukung.md` | Dokumen pendukung |
| `docs/03-data-uji-integrasi.md` | Data uji integrasi |
| `docs/04-pelaksanaan-uji-unit.md` | Pelaksanaan uji modul |
| `docs/05-analisis-data-integrasi.md` | Analisis hasil |
| `docs/06-laporan-hasil-integrasi.md` | Laporan hasil |
| `docs/07-dokumentasi-dan-arsip.md` | Dokumentasi & arsip |
| `docs/08-kesimpulan.md` | Kesimpulan |
| `Inventory-Integration-Testing.docx` | Gabungan Word |

## Sheets

- `sheets/equipment-sheet.csv`
- `sheets/test-case-sheet.csv`
- `sheets/result-sheet.csv`
- `sheets/defect-sheet.csv`

## Hasil

| Metrik | Nilai |
|---|---:|
| Build | 0 error |
| Test | 171 passed |
| Failed | 0 |
| Line coverage | 74.69% |
| Branch coverage | 60.17% |

## Catatan

Pengujian integrasi modul dilakukan pada level service + `AppDbContext` dengan
SQLite in-memory. Pengujian PostgreSQL nyata, API/controller, dan UI belum
dilaksanakan dan dicatat sebagai gap.
