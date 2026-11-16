# 07 - Dokumentasi dan Arsip Pengujian

## 1. Laporan

Laporan utama tersedia dalam:

- `docs/06-laporan-hasil-integrasi.md`
- `Inventory-Integration-Testing.docx`

## 2. Pelaporan

| Audiens | Media |
|---|---|
| Tim developer | Markdown + Word |
| QA | Sheets CSV + TRX |
| Manajemen | Ringkasan (00) |
| Audit | Arsip section 10 |

## 3. Arsip

| Artefak | Lokasi |
|---|---|
| Dokumen | `deliverables/10-integration-testing/docs/` |
| Word | `deliverables/10-integration-testing/Inventory-Integration-Testing.docx` |
| Sheets | `deliverables/10-integration-testing/sheets/` |
| TRX | `deliverables/10-integration-testing/results/test-results.trx` |
| Hasil CSV | `deliverables/10-integration-testing/results/test-results.csv` |
| Ringkasan | `deliverables/10-integration-testing/results/test-summary.csv` |
| Coverage | `deliverables/10-integration-testing/results/coverage.cobertura.xml` |
| Grafik | `deliverables/10-integration-testing/assets/integration-flow.png` |

## 4. Retensi

- Arsip disimpan bersama repository.
- Dipertahankan minimal sampai rilis berikutnya.
- Diperbarui setiap perubahan kode/perbaikan.

## 5. Akses

- Developer: read/write.
- QA: read.
- Manajemen: read ringkasan.
- Auditor: read-only.

## 6. Pemeliharaan

- Regenerasi TRX/coverage setiap test run.
- Perbarui sheets jika skenario berubah.
- Hapus artefak lama yang tidak relevan.
