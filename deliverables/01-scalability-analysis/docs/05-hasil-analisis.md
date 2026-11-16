# 05 - Hasil Analisis Skalabilitas

## 5.1 Ringkasan Temuan

1. Aplikasi Inventory adalah modular monolith ASP.NET Core MVC dengan fitur
   inventory lengkap, approval satu level, audit, dan soft delete.
2. Untuk skala **CV kecil–menengah**, arsitektur saat ini **memadai** pada
   deployment satu instance dengan database yang memadai.
3. Risiko skalabilitas utama:
   - query user per request (cookie validation),
   - kontensi stok dan penomoran dokumen,
   - laporan/stock card tanpa pagination,
   - migrasi otomatis saat startup,
   - belum siap multi-instance/HA,
   - belum ada observability dan load test.
4. Beban HTTP diperkirakan rendah (single-digit sampai belasan RPS), sehingga
   fokus optimalisasi harus pada **query database** dan **operasi tulis stok**.

## 5.2 Kesesuaian untuk Perusahaan CV

Aplikasi **cocok** untuk CV karena:

- Approval hanya satu level, sesuai struktur organisasi kecil.
- Modul inti inventory lengkap tanpa kompleksitas enterprise.
- Dapat berjalan pada server tunggal yang ekonomis.
- Mudah dioperasikan oleh tim IT kecil.

Namun, sebelum dipakai produksi:

- Pisahkan database dari server aplikasi.
- Siapkan backup dan uji restore.
- Tambah health check dan logging.
- Batasi akses dan gunakan HTTPS.

## 5.3 Rekomendasi Berdasarkan Prioritas

### Prioritas 1 — Wajib Sebelum Produksi

| No | Tindakan | Alasan |
|---|---|---|
| 1 | Pisahkan DB dari server aplikasi | Isolasi beban dan keamanan |
| 2 | Backup otomatis + uji restore | Mencegah kehilangan data |
| 3 | Health check + logging terstruktur | Deteksi gangguan lebih cepat |
| 4 | HTTPS + cookie secure | Keamanan kredensial |
| 5 | Batasi akses jaringan ke DB | Mengurangi risiko |

### Prioritas 2 — Optimasi Skala CV

| No | Tindakan | Dampak |
|---|---|---|
| 1 | Pagination Stock Card + default tanggal | Response lebih ringan |
| 2 | Batch query ApprovalService | Mengurangi N+1 |
| 3 | Index laporan movement/balance | Query lebih cepat |
| 4 | Cache opsi master data | Mengurangi query berulang |
| 5 | Retensi/arsip audit log | Menjaga ukuran DB |
| 6 | Atomic document number + retry | Mencegah bentrok nomor |
| 7 | Retry terkontrol untuk konflik stok | Pengalaman user lebih baik |

### Prioritas 3 — Pertumbuhan & HA

| No | Tindakan | Dampak |
|---|---|---|
| 1 | Shared Data Protection + multi-instance | Siap scale-out |
| 2 | Pisahkan migrasi dari startup | Deployment aman |
| 3 | `Inventory.Api` terpisah dari UI | Scaling independen |
| 4 | Read replica untuk laporan | Beban baca terpisah |
| 5 | Redis/distributed cache + queue | Skala dan background job |
| 6 | Metrics/tracing/alerting | Kapasitas terukur |

## 5.4 Roadmap Bertahap

### Fase 0 — Stabilisasi (0–1 bulan)
- Backup, HTTPS, health check, logging.
- Pisahkan database.
- Load test baseline.

### Fase 1 — Optimasi (1–3 bulan)
- Pagination, batch query, index, cache.
- Retensi audit.
- Atomic document number + retry stok.

### Fase 2 — Skala (3–6 bulan)
- Multi-instance + shared keys.
- Migrasi terpisah.
- Read replica/caching bila metrik menuntut.

### Fase 3 — API-first (6–12 bulan)
- Pisahkan `Inventory.Api` dan client HTTP.
- Load balancer, observability penuh, deployment independen.

## 5.5 Risiko

| Risiko | Dampak | Mitigasi |
|---|---|---|
| Data hilang | Tinggi | Backup + restore test |
| Kontensi stok saat peak | Sedang | Retry, monitoring, serialisasi per produk |
| Laporan lambat | Sedang | Index, pagination, arsip, read replica |
| Migrasi saat deploy | Sedang | Job migrasi terpisah |
| Cookie tidak konsisten multi-instance | Sedang | Shared Data Protection/JWT |
| Audit log membengkak | Rendah–sedang | Retensi dan arsip |

## 5.6 Metrik Keberhasilan

- p95 response halaman utama < 2 detik pada peak.
- Tidak ada kegagalan transaksi akibat konflik > 1% (target awal).
- Backup harian sukses dan restore teruji.
- Health check dan alert aktif.
- Tidak ada query > 1 detik pada laporan umum.

## 5.7 Kesimpulan Akhir

Aplikasi Inventory **sesuai untuk perusahaan kecil–menengah (CV)** dengan
approval satu level. Skalabilitas awal dapat dicapai melalui perbaikan
operasional dan optimasi query, tanpa harus langsung membangun arsitektur
terdistribusi. Pemisahan API/UI dan scale-out dapat dilakukan bertahap ketika
jumlah pengguna, transaksi, dan kebutuhan HA meningkat.
