# 02 - Analisis Masalah Skalabilitas

Analisis ini menghubungkan lingkup sistem (01 §1.1) dan lingkungan operasi
(01 §1.2) dengan potensi bottleneck saat data/pengguna bertambah.

## 2.1 Daftar Masalah dan Prioritas

| No | Masalah | Bukti | Dampak | Prioritas |
|---|---|---|---|---|
| S1 | Validasi principal per request ke database | `DependencyInjection.cs` `OnValidatePrincipal` memanggil `UserManager.GetUserAsync` | 1 query DB setiap request; beban DB naik linear | Tinggi |
| S2 | Kontensi saldo stok | `Stock.Version` concurrency token + update `Stocks` | Konflik/retry saat banyak transaksi pada produk/lokasi sama | Tinggi |
| S3 | Penomoran dokumen read-modify-write | `DocumentNumberService` mengambil lalu menaikkan `LastNumber` | Lost update/duplikasi nomor saat submit bersamaan | Tinggi |
| S4 | Approval N+1 | `ApprovalService` query 5 tipe + 1 query creator per item | Halaman approval lambat saat pending banyak | Sedang |
| S5 | Stock Card tanpa pagination | `GetStockCardAsync` mengembalikan seluruh movement | Memori/response besar untuk produk dengan histori panjang | Tinggi |
| S6 | Laporan movement/balance scan + join | `ReportService` join Stocks/Products/Locations + filter tanggal | Query mahal saat data bertahun-tahun | Sedang |
| S7 | Migrasi otomatis saat startup | `DatabaseBootstrapper.InitializeAsync` dipanggil di `Program.cs` | Start lambat, race/lock jika multi-instance | Tinggi |
| S8 | Belum multi-instance ready | Cookie + Data Protection key lokal | Login/logout tidak konsisten di >1 instance | Tinggi |
| S9 | Audit log tumbuh tanpa retensi | `AuditLogs` tanpa partisi/arsip | Tabel membesar, query audit melambat, backup membesar | Sedang |
| S10 | Cache master data belum ada | Opsi kategori/unit/produk/lokasi query tiap form | Query berulang, response form lebih lambat | Rendah |
| S11 | Tanpa health check/metrics | `Program.cs` tanpa health checks | Gangguan tidak terdeteksi, sulit kapasitas | Sedang |
| S12 | Tanpa rate limiting/compression | Pipeline default | Risiko abuse, transfer response lebih besar | Rendah |
| S13 | Export CSV dibatasi 200 baris | `ReportService.ExportCsvAsync` PageSize=200 | Kebutuhan export besar tidak terpenuhi | Sedang |
| S14 | Seeding/migrasi dan aplikasi satu proses | Satu host untuk semua peran | Sulit scaling independen dan deployment aman | Sedang |

## 2.2 Analisis per Masalah

### S1 - Validasi Principal per Request
Setiap request yang membawa cookie memicu validasi principal dan query user.
Pada 50 pengguna aktif dengan banyak request per menit, ini menambah beban DB
yang sebenarnya dapat dihindari dengan cache singkat (mis. 30–60 detik) atau
security stamp validation yang lebih ringan.

Rekomendasi: cache user aktif di memory/distributed cache, atau gunakan
`SecurityStampValidator` dengan interval.

### S2 - Kontensi Saldo Stok
Transaksi stok mengubah baris `Stocks` berdasarkan `(ProductId, LocationId)`.
Optimistic concurrency mencegah lost update, tetapi transaksi bersamaan pada
produk yang sama akan gagal dan perlu retry. Pada CV, hot item biasanya sedikit,
sehingga dampaknya terbatas; namun tetap perlu strategi retry dan pesan error
yang jelas.

Rekomendasi: retry terbatas pada `DbUpdateConcurrencyException`, pantau tingkat
konflik, dan pertimbangkan antrian/serialisasi per produk jika volume naik.

### S3 - Penomoran Dokumen
Pola baca-ubah-simpan pada `DocumentSequences` rentan bentrok saat dua user
submit bersamaan. Unique index `(Prefix, Year, Month)` membantu mencegah
duplikasi, tetapi pengalaman user bisa berupa error.

Rekomendasi: gunakan atomic increment (`UPDATE ... SET LastNumber = LastNumber + 1
RETURNING`), atau sequence per prefix/bulan, atau retry otomatis.

### S4 - Approval N+1
Untuk setiap item pending, service melakukan query creator terpisah. Dengan 100
pending, terjadi ±105 query. Ini masih dapat diterima pada skala CV, tetapi
mudah diperbaiki dengan join/projection sekali query.

Rekomendasi: satukan query creator dengan query utama, atau batch lookup by ID.

### S5 - Stock Card Tanpa Pagination
Kartu stok mengembalikan semua movement untuk produk (dan filter tanggal).
Produk dengan ribuan movement dapat membuat halaman lambat dan memori besar.

Rekomendasi: pagination server-side + batas tanggal default (mis. 3 bulan).

### S6 - Laporan Historis
Laporan movement dan balance melakukan join dan filter tanggal. Tanpa index
tambahan dan retensi, query akan melambat seiring pertumbuhan data.

Rekomendasi: index pada `(ProductId, CreatedAt)`, `(LocationId, CreatedAt)`,
`(CreatedAt)`; arsip data lama; read replica bila perlu.

### S7 - Migrasi Saat Startup
Setiap instance yang start akan mencoba migrasi. Pada satu instance masih
aman, tetapi menjadi risiko saat scale-out.

Rekomendasi: jalankan migrasi sebagai job terpisah sebelum deployment, dan
matikan auto-migrate di production.

### S8 - Kesiapan Multi-Instance
Cookie auth membutuhkan Data Protection key yang sama antar instance; jika tidak,
cookie dari instance A tidak valid di instance B.

Rekomendasi: simpan key di shared storage/database, atau gunakan BFF/JWT saat
API dipisah.

### S9 - Audit Log
Audit log penting untuk kepatuhan, tetapi tumbuh terus. Tanpa retensi, ukuran
tabel dan waktu backup meningkat.

Rekomendasi: retensi 1–3 tahun, arsip ke tabel/file terpisah, index yang sesuai.

### S10–S12 - Cache, Observability, Proteksi Dasar
Cache opsi master, health check, metrics, rate limiting, dan compression adalah
peningkatan murah yang memberi dampak besar pada stabilitas dan pengalaman user.

### S13 - Export CSV
Batasan 200 baris membuat export tidak lengkap untuk kebutuhan laporan besar.
Rekomendasi: streaming export per batch, atau background export.

### S14 - Pemisahan Peran Proses
Satu proses menjalankan UI, service, migrasi, dan seeding. Pemisahan API
(ADR 006) akan memudahkan scaling dan deployment independen.

## 2.3 Skenario Beban yang Perlu Diuji

1. **Peak hour**: 50 pengguna concurrently membuka dashboard/daftar transaksi.
2. **Burst submit**: 20 staff submit transaksi bersamaan pada produk populer.
3. **Opname**: satu lokasi dengan 5.000 produk, update counted quantity.
4. **Laporan**: stock card produk dengan 10.000 movement, filter 1 tahun.
5. **Approval**: 200 dokumen pending, halaman approval dibuka.
6. **End of month**: export CSV dan rekap laporan bersamaan.

## 2.4 Indikator yang Harus Dipantau

- Response time p95 per endpoint.
- Throughput request/detik.
- DB connection pool usage dan query duration.
- Tingkat `DbUpdateConcurrencyException`.
- Panjang antrian/lock pada tabel `Stocks` dan `DocumentSequences`.
- Ukuran database, pertumbuhan audit log, dan waktu backup.
