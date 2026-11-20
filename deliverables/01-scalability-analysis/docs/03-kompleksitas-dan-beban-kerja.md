## 2.2 Analisis Kompleksitas Aplikasi Sesuai dengan Kebutuhan Pemrosesan dan Jumlah Data/Pengguna yang Akan Terlibat

### Seberapa kompleks aplikasinya?

| Bagian | Tingkat | Alasan |
|---|---|---|
| Data dasar dan pengguna | Rendah–sedang | Isi dan ubah data, atur hak akses |
| Barang masuk, keluar, pindah | Sedang | Perlu validasi, persetujuan, dan catatan stok |
| Penyesuaian dan stok opname | Sedang–tinggi | Banyak barang dicek sekaligus |
| Laporan dan riwayat | Sedang | Menggabungkan data dari beberapa bagian |

Aplikasi ini tidak melakukan perhitungan berat. Bagian yang paling perlu dijaga
adalah ketepatan stok dan kecepatan saat membuka laporan.

### Perkiraan pengguna dan data

| Hal | Perkiraan |
|---|---|
| Karyawan | 20–100 orang |
| Pengguna aktif | 10–50 orang |
| Barang aktif | 500–10.000 |
| Lokasi atau gudang | 2–20 |
| Transaksi per bulan | 1.000–10.000 |
| Lama data disimpan | 3 tahun |

### Perkiraan pertumbuhan

- Sekitar 240.000 baris transaksi per tahun, dan menjadi jutaan dalam 3 tahun.
- Jumlah itu masih wajar untuk database standar yang dirawat dengan baik.
- Beban harian tergolong ringan; yang perlu dijaga adalah laporan besar dan
  transaksi yang terjadi bersamaan pada barang yang sama.

**Kesimpulan:** tingkat kerumitannya sedang dan masih terkendali untuk tim kecil.
