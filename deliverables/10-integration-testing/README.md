# 10 - Integration Testing Documentation

## Konteks

Dokumentasi peralatan, lingkungan, data uji integrasi, pelaksanaan uji modul,
analisis hasil, laporan, dan arsip pengujian.

## Dokumen

`docs/` disusun mengikuti 6 langkah kerja:

| File | Butir |
|---|---|
| `docs/01-peralatan-dan-lingkungan.md` | 1. Peralatan dan dokumen pendukung |
| `docs/02-data-uji-integrasi.md` | 2. Identifikasi dan pembangkitan data uji |
| `docs/03-pelaksanaan-pengujian.md` | 3. Pelaksanaan dan pencatatan hasil |
| `docs/04-analisis-data-pengujian.md` | 4. Analisis modul dan hasil |
| `docs/05-laporan-hasil.md` | 5. Laporan hasil pengujian |
| `docs/06-dokumentasi-dan-arsip.md` | 6. Pelaporan dan pengarsipan |
| `docs/07-third-party-integration-testing.md` | 7. Integration test procurement, stock in, approval, third party |
| `10-Inventory-Integration-Testing.docx` | Gabungan Word |

## Sheets

- `sheets/equipment-sheet.csv`
- `sheets/test-case-sheet.csv`
- `sheets/result-sheet.csv`
- `sheets/result-sheet-third-party.csv`
- `sheets/defect-sheet.csv`

## Hasil

| Metrik | Nilai |
|---|---:|
| Build | 0 error |
| Test unit/modul | 171 (artefak 09) / 182 run terbaru |
| Test integrasi procurement & approval | 9 passed |
| Test integrasi third party (HTTP stub) | 5 passed |
| Total test integrasi | 14 passed |
| Failed | 0 |
| Line coverage | 74,69% |
| Branch coverage | 60,17% |

## Catatan

Pengujian integrasi modul dilakukan pada level service + `AppDbContext` dengan
SQLite in-memory. Pengujian PostgreSQL nyata, API/controller, dan UI belum
dilaksanakan dan dicatat sebagai gap.
