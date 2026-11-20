# 5. Evaluasi Hasil Uji Coba

![Ringkasan Hasil Uji](assets/test-summary.png)

## a. Catat hasil uji coba

| Metrik | Nilai |
|---|---:|
| Total test | 171 |
| Lulus | 171 |
| Gagal | 0 |
| Dilewati | 0 |
| Tingkat kelulusan | 100% |
| Durasi | ±10 detik |
| Cakupan baris | 74,69% |
| Cakupan cabang | 60,17% |

**Hasil per kelompok:** validasi, ledger, barang masuk/keluar, transfer,
adjustment/opname, katalog, supplier/lokasi, persetujuan/laporan/audit, dan
pengguna semuanya **lulus**.

## b. Analisis hasil uji coba

| Bagian | Cakupan baris | Cakupan cabang |
|---|---:|---:|
| Application | 73,79% | 59,91% |
| Infrastructure | 72,06% | 61,60% |
| Domain | 77,89% | 50,00% |
| Contracts | 84,80% | 0%* |

\*Cabang pada DTO umumnya tidak relevan; cakupan baris lebih penting.

**Area yang cakupannya masih rendah:**

- Daftar, detail, ubah, dan hapus pada Transfer/Adjustment.
- Detail Stock In/Out.
- Test database PostgreSQL nyata.
- Test halaman web dan API.

**Kesimpulan analisis:** unit test sehat dan konsisten. Kekurangan yang ada
bukan kegagalan test, melainkan **cakupan pengujian yang perlu ditambah**.

## c. Laporkan prosedur uji coba

| Item | Hasil |
|---|---|
| Build | 0 error |
| Test | 171 lulus, 0 gagal |
| Durasi | ±10 detik |
| Cakupan baris | 74,69% |
| Cakupan cabang | 60,17% |
| Bukti | TRX, CSV, Cobertura, ringkasan |

**Langkah pelaporan:**

1. Build solution.
2. Jalankan unit test dengan TRX dan coverage.
3. Simpan semua file hasil.
4. Analisis cakupan dan kegagalan.
5. Perbaiki lalu ulangi sampai 0 gagal.
6. Perbarui dokumen.

## d. Selesaikan kesalahan/error

| ID | Temuan | Perbaikan | Status |
|---|---|---|---|
| DEF-01 | Stok gagal saat beberapa perubahan pada data sama | Periksa data lokal sebelum menambah stok | Selesai |
| DEF-02 | Ubah detail transaksi memicu konflik data | Tambahkan detail baru secara eksplisit | Selesai |
| DEF-03 | Response kategori tidak membawa pembuat | Tambahkan field pembuat | Selesai |
| DEF-04 | Jumlah stok tampil desimal | Ubah ke bilangan bulat | Selesai |
| DEF-05 | Error Razor palsu di IDE | Clean, rebuild, restart | Selesai |
| DEF-06 | Build gagal karena file terkunci | Hentikan debugger lalu build | Selesai |
| DEF-07 | DocFX tidak ditemukan | Pulihkan tool manifest | Selesai |

**Rekomendasi lanjutan:**

| Prioritas | Tindakan |
|---|---|
| Tinggi | Integration test PostgreSQL |
| Tinggi | API/contract test |
| Sedang | Tambah test daftar/detail/ubah/hapus |
| Sedang | Naikkan cakupan cabang |
| Rendah | Uji tampilan web (smoke test) |

**Kesimpulan:** tidak ada kegagalan test pada eksekusi terakhir. Kesalahan
historis sudah diperbaiki dan diverifikasi. Untuk kesiapan produksi, perlu
ditambah integration test, API test, dan otomatisasi CI.
