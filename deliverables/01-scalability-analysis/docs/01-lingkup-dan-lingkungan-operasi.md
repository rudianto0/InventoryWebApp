# 1. Mengumpulkan Kebutuhan Skalabilitas

## 1.1 Identifikasi Lingkup (Scope) Sistem

Aplikasi Inventory adalah aplikasi web untuk mencatat barang masuk, barang
keluar, dan perpindahan barang antar lokasi. Tujuannya agar stok selalu akurat,
mudah diperiksa, dan tidak lagi bergantung pada pencatatan manual.

**Siapa yang memakai**

| Pengguna | Tugas |
|---|---|
| Admin | Mengatur pengguna dan data dasar |
| Staff | Mencatat transaksi stok |
| SPV | Menyetujui transaksi dan memantau stok |
| Manager | Melihat laporan dan ringkasan |
| Auditor | Memeriksa riwayat tanpa mengubah data |

**Fitur utama**

- Login dan hak akses sesuai peran.
- Data barang, kategori, satuan, gudang, dan supplier.
- Barang masuk, keluar, pindah lokasi, penyesuaian, dan stok opname.
- Persetujuan transaksi satu tingkat.
- Laporan stok dan riwayat perubahan.

**Mengapa operasinya cukup kompleks**

- Satu transaksi bisa berisi banyak barang sekaligus.
- Stok dihitung per barang dan per lokasi.
- Setiap perubahan harus tercatat agar bisa diaudit.
- Ada proses persetujuan dan pengecekan stok berkala.

**Yang tidak termasuk**

- Tidak untuk banyak perusahaan sekaligus.
- Tidak terhubung ke marketplace.
- Tidak memakai alat barcode khusus.
- Persetujuan hanya satu tingkat.

## 1.2 Identifikasi Lingkungan Operasi Aplikasi

Aplikasi dipakai lewat browser di jaringan kantor. Cukup satu server aplikasi
dan satu server database terpisah, sehingga cocok untuk tim IT kecil.

```
Pengguna (browser) -> Server aplikasi -> Server database
```

**Hal yang perlu diperhatikan**

- Tim IT kecil, jadi sistem harus mudah dirawat.
- Anggaran terbatas, jadi mulai dari satu server lebih dulu.
- Data harus aman dan hanya bisa diakses pihak berwenang.
- Jumlah data akan terus bertambah seiring waktu.
