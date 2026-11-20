# 1. Mempersiapkan Dokumentasi Peralatan dan Lingkungan

## 1.1 Tentukan peralatan pengujian sesuai dengan kebutuhan

| No | Peralatan | Versi | Fungsi | Status |
|---|---|---|---|---|
| 1 | Windows 11 | - | Sistem operasi | Tersedia |
| 2 | .NET SDK | 10.0.404 | Build dan menjalankan test | Tersedia |
| 3 | .NET Runtime | 10.0.15 | Menjalankan aplikasi | Tersedia |
| 4 | xUnit | 2.9.3 | Kerangka pengujian | Tersedia |
| 5 | FluentAssertions | 8.11.0 | Memeriksa hasil uji | Tersedia |
| 6 | EF Core SQLite | 10.0.15 | Database uji sementara | Tersedia |
| 7 | coverlet.collector | 10.1.0 | Mengukur cakupan kode | Tersedia |
| 8 | Rider / Visual Studio | terbaru | Debug dan test explorer | Tersedia |
| 9 | DBeaver | terbaru | Memeriksa database | Tersedia |

**Lingkungan pengujian:**

| Aspek | Nilai |
|---|---|
| Sistem operasi | Windows 11 |
| Runtime | .NET 10.0.15 |
| Konfigurasi | Debug |
| Database uji | SQLite in-memory |
| Hasil pengujian | `results/` |
| CI otomatis | Belum tersedia |

**Kebutuhan perangkat:** CPU 2 core (disarankan 4), RAM 4 GB (disarankan 8),
dan ruang disk 2 GB. SQLite dipilih agar pengujian cepat dan tidak bergantung
pada server database.

## 1.2 Siapkan dokumen pendukung pengujian

| Dokumen | Isi | Lokasi |
|---|---|---|
| Rencana uji | Ruang lingkup, jadwal, tools | Dokumen ini |
| Lembar test case | Skenario dan hasil yang diharapkan | `sheets/test-case-sheet.csv` |
| Lembar hasil | Hasil nyata setiap test | `sheets/result-sheet.csv` |
| Lembar cacat | Ketidaksesuaian dan perbaikan | `sheets/defect-sheet.csv` |
| Lembar peralatan | Daftar peralatan uji | `sheets/equipment-sheet.csv` |
| Hasil test | TRX dan CSV | `results/` |
| Laporan cakupan | Baris dan cabang | `results/coverage-*` |
| Skema database | Tabel dan relasi | `deliverables/02` |

**Aturan pengisian lembar:**

- Setiap test memiliki ID unik.
- Hasil yang diharapkan ditulis sebelum pengujian.
- Hasil nyata diisi setelah pengujian.
- Status diisi Pass, Fail, Blocked, atau Not Run.
- Bukti diisi nama test atau file TRX.
