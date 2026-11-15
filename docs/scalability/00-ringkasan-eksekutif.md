# 00 - Ringkasan Eksekutif

## Tujuan

Menilai kesiapan skalabilitas aplikasi Inventory untuk kebutuhan perusahaan
kecil–menengah (CV), serta mengidentifikasi risiko dan langkah peningkatan yang
paling relevan.

## Karakteristik Aplikasi

- ASP.NET Core MVC (.NET 10) + Razor Views, EF Core 10.
- Database PostgreSQL atau SQL Server (dipilih via konfigurasi).
- Modular monolith dengan boundary `Inventory.Contracts` dan `Inventory.Client`.
- 12 modul fitur: auth, user, catalog, lokasi, supplier, stock in/out/transfer,
  adjustment, opname, dashboard/report, audit.
- Approval satu level; audit dan soft delete aktif.
- 171 unit test; belum ada load test atau integration test performa.

## Kesimpulan Utama

Aplikasi **layak untuk skala CV** pada tahap awal, dengan catatan:

1. **Cukup untuk beban rendah–sedang** (10–50 pengguna bersamaan, ribuan
   transaksi per bulan) jika dijalankan pada server yang memadai dan database
   terpisah dari aplikasi.
2. **Belum siap multi-instance tanpa penyesuaian** karena cookie/Data Protection
   belum dibagikan dan migrasi berjalan otomatis saat startup.
3. **Bottleneck utama ada di database dan operasi stok**, bukan di UI:
   kontensi saldo per produk/lokasi, penomoran dokumen, dan laporan historis.
4. **Beberapa query berpotensi mahal** saat data bertambah: approval N+1,
   stock card tanpa pagination, dan audit log tanpa retensi.
5. **Belum ada observability** (health check, metrics, tracing) sehingga
   kapasitas aktual belum terukur.

## Rekomendasi Bertahap

### Quick Win (0–1 bulan)
- Tambah health check dan logging terstruktur.
- Tambah pagination pada Stock Card.
- Batch query pada ApprovalService.
- Tambah caching singkat untuk opsi master data.
- Pisahkan database dari server aplikasi.
- Backup otomatis + uji restore.

### Jangka Menengah (1–3 bulan)
- Shared Data Protection keys dan konfigurasi siap multi-instance.
- Pisahkan proses migrasi dari startup aplikasi.
- Optimasi index dan query laporan.
- Tambah retensi/arsip audit log.
- Load test dengan skenario transaksi dan laporan.

### Jangka Panjang (3–12 bulan)
- Pisahkan `Inventory.Api` dan UI (sesuai ADR 006).
- Load balancer + 2 instance aplikasi.
- Read replica untuk laporan.
- Redis/distributed cache dan background worker.
- Monitoring terpusat dan alerting.

## Tingkat Kesiapan

| Aspek | Status | Catatan |
|---|---|---|
| Skala CV awal | Siap | Dengan server tunggal yang memadai |
| Skala CV berkembang | Siap dengan penyesuaian | DB terpisah, caching, optimasi query |
| Multi-instance / HA | Belum siap | Perlu shared keys, migrasi terpisah |
| Observability | Belum siap | Perlu health check, metrics, tracing |
| Load test | Belum ada | Perlu validasi kapasitas |
