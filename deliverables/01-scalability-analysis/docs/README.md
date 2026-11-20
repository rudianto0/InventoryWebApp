# Analisis Kebutuhan Skalabilitas Aplikasi Inventory

Dokumen ini menjelaskan kebutuhan, alasan, dan hasil analisis yang menjadi
dasar perancangan solusi aplikasi Inventory, disusun sebagai bahan presentasi
teknis 10 menit kepada asesor mengikuti butir:

1. Mengumpulkan kebutuhan skalabilitas
   1.1 Identifikasi lingkup (scope) sistem
   1.2 Identifikasi lingkungan operasi aplikasi
2. Menganalisis kebutuhan skalabilitas
   2.1 Analisis masalah skalabilitas
   2.2 Analisis kompleksitas aplikasi
   2.3 Analisis kebutuhan perangkat keras
   2.4 Dokumentasi hasil analisis

## Daftar Dokumen

| File | Butir |
|---|---|
| `01-lingkup-dan-lingkungan-operasi.md` | 1.1 dan 1.2 |
| `02-analisis-masalah-skalabilitas.md` | 2.1 |
| `03-kompleksitas-dan-beban-kerja.md` | 2.2 |
| `04-kebutuhan-perangkat-keras.md` | 2.3 |
| `05-hasil-analisis.md` | 2.4 |
| `../01-Inventory-Scalability-Analysis.docx` | Gabungan seluruh dokumen |

## Asumsi

Angka pada dokumen adalah estimasi awal dan harus divalidasi dengan data
operasional nyata serta load test:

- 20–100 karyawan, 10–50 pengguna aktif pada jam sibuk.
- 500–10.000 produk aktif, 2–20 lokasi/gudang.
- 1.000–10.000 transaksi per bulan, puncak pada akhir bulan.
- Retensi data transaksi dan audit 3 tahun.
- Deployment awal: satu server aplikasi + satu database terpisah.
