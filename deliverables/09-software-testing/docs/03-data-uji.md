# 3. Persiapkan Data Uji

## a. Identifikasi data uji unit test

| Sumber data | Isi |
|---|---|
| `SeedBasicsAsync` | Kategori, satuan, produk, gudang, supplier |
| `SeedStockAsync` | Saldo stok per produk dan lokasi |
| `SeedLocationAsync` | Lokasi aktif/nonaktif dan induknya |
| `SeedProductAsync` | Produk aktif/nonaktif |
| `IdentityFixture` | Pengguna, peran, kata sandi |
| `FakeCurrentUser` | Pengguna yang sedang login |
| `FakeUserDirectory` | Atasan dan status aktif |
| `FakeDocumentNumbers` | Nomor dokumen tetap untuk pengujian |

**Data dasar:**

```text
Kategori : General
Satuan   : PCS
Produk   : SKU-1 / Product 1
Cabang   : HQ
Gudang   : WH1, WH2
Supplier : SUP
```

## b. Bangkitkan data uji unit test

Setiap test membuat data sendiri agar tidak saling mengganggu:

1. Buat database sementara `SQLite in-memory`.
2. Bentuk tabel dengan `EnsureCreated`.
3. Isi data awal memakai helper.
4. Gunakan ID tetap untuk data yang saling berhubungan.
5. Hapus database setelah test selesai.

**Variasi data yang dibuat:**

| Variasi | Contoh |
|---|---|
| Valid | Jumlah 5, produk dan lokasi aktif |
| Batas | Jumlah 1, halaman 1 dan 200 |
| Tidak valid | Jumlah 0/negatif, SKU duplikat, data nonaktif |
| Status | Produk terhapus, dokumen Draft/Submitted/Approved |
| Hak akses | Pemilik, bukan pemilik, admin, penyetuju |
| Bersamaan | Dua perubahan pada produk dan lokasi sama |

**Contoh data uji:**

| Pengujian | Data |
|---|---|
| Stok keluar tidak cukup | Stok 2, diminta 5 |
| Bukan penyetuju | Pembuat 10, penyetuju 20, aktor 99 |
| SKU duplikat | `SKU-1` dipakai dua kali |
| Produk terhapus | Produk dan stok, lalu ditandai terhapus |
| Ukuran halaman 500 | Dibatasi menjadi 200 |

**Aturan data:**

- Tidak memakai data asli produksi.
- Tidak menyimpan data pribadi.
- Data dihapus setelah pengujian.
- Waktu memakai UTC.
- Tidak bergantung pada nomor urut database.
