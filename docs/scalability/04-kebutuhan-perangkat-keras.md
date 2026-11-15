# 04 - Analisis Kebutuhan Perangkat Keras

Perangkat keras diestimasi berdasarkan asumsi beban pada dokumen 03. Angka ini
adalah baseline, bukan hasil load test.

## 4.1 Prinsip Sizing

1. Aplikasi ASP.NET Core relatif ringan; bottleneck utama adalah database.
2. Pisahkan aplikasi dan database bila memungkinkan.
3. Gunakan SSD untuk database karena banyak operasi baca/tulis transaksional.
4. Sisakan headroom CPU/RAM 30–50% untuk peak dan pertumbuhan.
5. Backup dan monitoring bukan opsional.

## 4.2 Tier Perangkat Keras

### Tier 1 — Awal / CV Kecil

Cocok untuk: 10–20 pengguna aktif, 1.000–3.000 transaksi/bulan, 500–2.000 produk.

| Komponen | Spesifikasi |
|---|---|
| Aplikasi | 2 vCPU, 4 GB RAM, 40 GB SSD |
| Database | 2 vCPU, 8 GB RAM, 100 GB SSD (bisa satu server) |
| Jaringan | 100 Mbps, HTTPS |
| Backup | Harian, retensi 7–30 hari |
| Catatan | Cukup untuk single instance; DB disarankan terpisah |

### Tier 2 — CV Berkembang

Cocok untuk: 20–50 pengguna aktif, 3.000–10.000 transaksi/bulan, 2.000–10.000 produk.

| Komponen | Spesifikasi |
|---|---|
| Aplikasi | 4 vCPU, 8 GB RAM, 80 GB SSD |
| Database | 4 vCPU, 16 GB RAM, 250 GB SSD |
| Jaringan | 1 Gbps internal |
| Backup | Harian + WAL/transaction log, retensi 30–90 hari |
| Catatan | DB terpisah, monitoring, load test |

### Tier 3 — Pertumbuhan / Multi-Cabang

Cocok untuk: 50–100 pengguna aktif, >10.000 transaksi/bulan, banyak lokasi.

| Komponen | Spesifikasi |
|---|---|
| Aplikasi | 2–3 instance × (4 vCPU, 8 GB RAM) di belakang load balancer |
| Database | 8 vCPU, 32 GB RAM, 500 GB SSD + read replica |
| Cache | Redis 1–2 GB |
| Backup | Harian + PITR, retensi 90–365 hari |
| Catatan | Shared Data Protection, migrasi terpisah, observability |

## 4.3 Estimasi Storage

Rumus:

```
storage = (jumlah baris × ukuran rata-rata baris)
        + index (30–60%)
        + audit log
        + backup (2–3× ukuran database)
```

Contoh 3 tahun, skenario menengah:

- Detail/ movement: ±1–2 juta baris × 200–400 byte ≈ 400–800 MB.
- Audit log: ±1–2 juta baris × 500 byte ≈ 500 MB–1 GB.
- Total data + index: ±2–4 GB.
- Backup dan pertumbuhan: sediakan **100–250 GB** agar aman.

Untuk CV, storage bukan bottleneck utama; **IOPS dan latency** lebih penting.

## 4.4 Koneksi Database

Rumus:

```
total_koneksi = jumlah_instance × connection_pool_per_instance
```

- Npgsql default pool: 100 per instance.
- SQL Server default pool: 100 per instance.
- Pastikan `total_koneksi < max_connections` database dengan reserve 20–30%.

Contoh:

- 2 instance × pool 50 = 100 koneksi; set `max_connections` ≥ 150.
- Jika hanya 1 instance, pool 50–100 sudah cukup untuk beban CV.

## 4.5 Jaringan

- HTTPS wajib untuk login dan data stok.
- Latensi aplikasi–database < 2 ms (satu jaringan/region).
- Untuk multi-cabang, aplikasi sebaiknya dipusatkan, bukan DB per cabang.

## 4.6 High Availability (Opsional untuk CV)

| Kebutuhan | Rekomendasi |
|---|---|
| Uptime 99% | 1 app + 1 DB dengan backup dan monitoring |
| Uptime 99.5% | 2 app instance + LB + DB backup/PITR |
| Uptime 99.9% | Multi-AZ, read replica, failover DB, monitoring 24/7 |

Untuk CV, prioritas realistis: **backup + restore test + monitoring**, bukan
cluster mahal.

## 4.7 Rekomendasi Awal

1. Mulai dari Tier 1/2 dengan database terpisah.
2. Gunakan SSD dan backup otomatis.
3. Sediakan monitoring CPU, RAM, disk, koneksi DB, dan query lambat.
4. Lakukan load test sebelum menaikkan skala.
5. Naikkan tier berdasarkan metrik, bukan perkiraan.
