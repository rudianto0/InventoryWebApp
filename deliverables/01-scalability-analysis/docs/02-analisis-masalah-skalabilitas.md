# 2. Menganalisis Kebutuhan Skalabilitas

## 2.1 Analisis Masalah Skalabilitas Berdasar Lingkup dan Lingkungan Operasi Sistem

Skalabilitas berarti aplikasi tetap nyaman dipakai walaupun jumlah pengguna dan
data semakin bertambah. Karena aplikasi berjalan lewat web, bebannya muncul
saat banyak orang memakai bersamaan, saat stok diubah pada barang yang sama,
dan saat membuka laporan besar.

| Masalah | Penyebab | Jika dibiarkan | Solusi |
|---|---|---|---|
| Data makin menumpuk | Transaksi bertambah setiap bulan | Aplikasi dan backup makin lambat | Simpan data 3 tahun, rapikan arsip lama |
| Stok bisa tidak cocok | Dua orang mengubah barang yang sama | Angka stok salah | Sistem memeriksa dan mengulang otomatis |
| Nomor dokumen bentrok | Banyak dokumen dibuat bersamaan | Nomor ganda | Penomoran otomatis yang terkunci |
| Halaman riwayat berat | Riwayat barang sangat panjang | Halaman lambat dibuka | Tampilkan per halaman dan filter tanggal |
| Laporan lambat | Membaca data bertahun-tahun | Menunggu lama saat rapat | Ringkas data dan batasi rentang tanggal |
| Jam sibuk melambat | Banyak pengguna di waktu yang sama | Aplikasi terasa lambat | Kurangi pekerjaan berulang yang tidak perlu |
| Butuh cadangan | Sewaktu-waktu server bermasalah | Pekerjaan bisa terhenti | Siapkan backup dan peningkatan bertahap |

**Kesimpulan**

Tidak perlu sistem yang rumit dan mahal. Dengan rancangan yang rapi sejak awal,
aplikasi sederhana sudah cukup untuk kebutuhan perusahaan kecil–menengah dan
masih bisa berkembang ketika pengguna bertambah.
