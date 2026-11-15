# Analisis Skalabilitas Aplikasi Inventory

Folder ini berisi analisis kebutuhan skalabilitas aplikasi Inventory Web App
berdasarkan kondisi kode saat ini. Dokumen disusun mengikuti butir:

1. Mengumpulkan kebutuhan skalabilitas
   1.1 Identifikasi lingkup (scope) sistem
   1.2 Identifikasi lingkungan operasi aplikasi
2. Menganalisis kebutuhan skalabilitas
   2.1 Analisis masalah skalabilitas
   2.2 Analisis kompleksitas aplikasi
   2.3 Analisis kebutuhan perangkat keras
   2.4 Dokumentasi hasil analisis

## Daftar Dokumen

| File | Isi |
|---|---|
| `00-ringkasan-eksekutif.md` | Ringkasan temuan dan rekomendasi utama |
| `01-lingkup-dan-lingkungan-operasi.md` | Butir 1.1 dan 1.2 |
| `02-analisis-masalah-skalabilitas.md` | Butir 2.1 |
| `03-kompleksitas-dan-beban-kerja.md` | Butir 2.2 |
| `04-kebutuhan-perangkat-keras.md` | Butir 2.3 |
| `05-hasil-analisis.md` | Butir 2.4 |
| `Inventory-Scalability-Analysis.docx` | Gabungan seluruh dokumen dalam format Word |

## Konteks Aplikasi

Aplikasi Inventory adalah aplikasi web untuk mengelola persediaan barang:
stok masuk/keluar, transfer, adjustment, opname, approval, laporan, dan audit.
Aplikasi ditujukan untuk perusahaan kecil–menengah (CV) dengan approval satu
level (Staff → SPV/Manager), bukan enterprise multi-level.

## Asumsi

Angka pada dokumen ini adalah **estimasi awal** dan harus divalidasi dengan data
operasional nyata serta load test:

- 20–100 karyawan, 10–50 pengguna aktif bersamaan pada jam sibuk.
- 500–10.000 produk aktif, 2–20 lokasi/gudang.
- 1.000–10.000 transaksi per bulan, dengan puncak pada akhir bulan.
- Retensi data transaksi dan audit 3 tahun.
- Deployment awal: satu server aplikasi + satu database (VM/on-prem/cloud kecil).
- SLA awal: uptime 99% pada jam kerja, respons halaman < 2 detik.

## Catatan Keamanan

Dokumen ini tidak memuat connection string, password, atau data sensitif lain.
