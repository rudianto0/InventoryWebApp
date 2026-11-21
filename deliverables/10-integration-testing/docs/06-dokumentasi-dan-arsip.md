# 6. Melaporkan Dokumentasi Pengujian

## 6.1 Dokumentasikan hasil pengujian menjadi laporan

Laporan disusun dengan struktur berikut:

1. Identitas pengujian.
2. Peralatan dan lingkungan.
3. Data yang diuji.
4. Pelaksanaan.
5. Hasil dan analisis.
6. Kesimpulan dan rekomendasi.

Laporan tersedia dalam bentuk:

- Markdown pada folder `docs/`.
- Word `10-Inventory-Integration-Testing.docx`.

## 6.2 Laporkan dokumentasi hasil pengujian

| Penerima | Media |
|---|---|
| Tim developer | Markdown dan Word |
| QA | CSV sheets dan TRX |
| Manajemen | Ringkasan hasil |
| Auditor | Arsip lengkap section 10 |

**Isi laporan untuk manajemen:** jumlah test, tingkat kelulusan, cakupan, dan
kesimpulan singkat.

**Isi laporan untuk QA:** skenario, data, hasil nyata, bukti TRX, dan daftar
cacat.

## 6.3 Arsipkan dokumentasi hasil pengujian

| Artefak | Lokasi |
|---|---|
| Dokumen | `docs/` |
| Word | `10-Inventory-Integration-Testing.docx` |
| Lembar uji | `sheets/` |
| Lembar hasil third party | `sheets/result-sheet-third-party.csv` |
| Hasil TRX | `results/test-results.trx` |
| Hasil CSV | `results/test-results.csv` |
| Ringkasan | `results/test-summary.csv` |
| Cakupan | `results/coverage.cobertura.xml` |
| Gambar alur | `assets/integration-flow.png` |

**Retensi dan akses:**

- Arsip disimpan bersama repository.
- Dipertahankan minimal sampai rilis berikutnya.
- Diperbarui setiap ada perubahan kode atau perbaikan.
- Developer dapat membaca dan menulis.
- QA, manajemen, dan auditor memiliki akses baca.

**Kesimpulan:** seluruh hasil pengujian integrasi sudah dilaporkan dan
diarsipkan sehingga dapat diperiksa kembali saat audit atau pengembangan
berikutnya.
