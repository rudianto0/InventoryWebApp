# 2. Persiapkan Dokumentasi Uji Coba

## b. Tentukan kebutuhan untuk uji coba

| Dokumen | Isi |
|---|---|
| Rencana uji | Tujuan, ruang lingkup, tools, jadwal |
| Skenario uji | Daftar hal yang akan diuji |
| Test case | Langkah, data, hasil yang diharapkan |
| Data uji | Data valid, batas, dan tidak valid |
| Hasil uji | Lulus/gagal dan bukti |
| Laporan bug | Kesalahan dan perbaikan |
| Laporan cakupan | Persentase kode yang diuji |

**Format test case:**

| Field | Keterangan |
|---|---|
| ID | `TC-<MODUL>-NNN` |
| Modul | ID modul yang diuji |
| Judul | Ringkasan pengujian |
| Kondisi awal | Data dan pengguna yang disiapkan |
| Langkah | Urutan tindakan |
| Hasil diharapkan | Hasil yang benar |
| Hasil nyata | Hasil saat dijalankan |
| Status | Lulus atau gagal |
| Bukti | Log atau file hasil |

## c. Laksanakan uji coba dengan variasi kondisi

| Variasi | Contoh |
|---|---|
| Data valid | Produk lengkap, jumlah lebih dari 0 |
| Batas | Jumlah 1, ukuran halaman 1 dan 200 |
| Tidak valid | Kosong, negatif, duplikat |
| Status data | Aktif, nonaktif, sudah dihapus |
| Hak akses | Staff, SPV, Admin, atasan tidak valid |
| Bersamaan | Dua perubahan stok pada waktu sama |
| Data kosong | Tidak ada produk atau stok |
| Kesalahan database | Aturan unik atau konflik data |

## d. Buat skenario uji coba

| Kelompok | File test | Jumlah |
|---|---|---:|
| Validasi dan ledger | `GuardAndLedgerTests`, `CoreTests` | 15+ |
| Barang masuk dan keluar | `StockInServiceTests`, `StockOutServiceTests` | 20+ |
| Transfer | `StockTransferServiceTests` | 12 |
| Adjustment dan opname | `StockAdjustmentServiceTests` | 30+ |
| Katalog | `CatalogServiceTests` | 30+ |
| Supplier dan lokasi | `SupplierAndLocationTests` | 25+ |
| Persetujuan, laporan, audit | `ApprovalServiceTests`, `ReportServiceTests` | 30+ |
| Pengguna dan hak akses | `UserServiceTests` | 12 |
| **Total** | | **171** |

**Contoh skenario:**

| ID | Skenario | Hasil yang diharapkan |
|---|---|---|
| TC-LED-001 | Tambah stok 10 | Saldo menjadi 10 |
| TC-LED-002 | Keluar melebihi stok | Ditolak |
| TC-SIN-001 | Simpan barang masuk valid | Tersimpan sebagai draft |
| TC-SIN-002 | Setujui oleh bukan penyetuju | Ditolak |
| TC-SOUT-001 | Keluar dengan stok kurang | Ditolak |
| TC-TRF-001 | Transfer antar lokasi | Saldo asal turun, tujuan naik |
| TC-CAT-001 | SKU duplikat | Ditolak |
| TC-SUP-001 | Email tidak valid | Ditolak |
| TC-LOC-001 | Lokasi induk berputar | Ditolak |
| TC-USR-001 | Menurunkan admin terakhir | Ditolak |
