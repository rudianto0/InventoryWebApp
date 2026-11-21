# 5. Melaporkan Hasil Pengujian Integrasi

## 5.1 Catat peralatan yang digunakan ke dalam lembar peralatan

Peralatan dicatat pada `sheets/equipment-sheet.csv`.

| Peralatan | Versi | Fungsi |
|---|---|---|
| .NET SDK | 10.0.404 | Build dan menjalankan test |
| .NET Runtime | 10.0.15 | Menjalankan aplikasi |
| xUnit | 2.9.3 | Kerangka pengujian |
| FluentAssertions | 8.11.0 | Pemeriksaan hasil |
| EF Core SQLite | 10.0.15 | Database uji |
| coverlet.collector | 10.1.0 | Cakupan kode |
| Rider / Visual Studio | terbaru | Debug dan test explorer |
| DBeaver | terbaru | Pemeriksaan database |

## 5.2 Catat kondisi yang terjadi selama pengujian

- Build berhasil tanpa error.
- Database sementara dibuat baru untuk setiap test.
- Tidak ada akses database eksternal atau jaringan.
- Pengujian berjalan sekitar 7 detik.
- Tidak ada test gagal atau dilewati.
- Seluruh hasil disimpan di folder `results/`.

## 5.3 Catat data yang diimplementasikan dan data hasil pengujian

| Test ID | Data masukan | Hasil |
|---|---|---|
| INT-01 | Jumlah 7, pembuat 10, penyetuju 20 | Stok 7, Approved |
| INT-02 | Stok 10, jumlah 4 | Stok 6 |
| INT-03 | Stok 2, jumlah 5 | Ditolak |
| INT-04 | WH1 10, jumlah 4 | WH1 6, WH2 4 |
| INT-05 | Sistem 10, hitung 7 | Selisih -3 |
| INT-06 | Sistem 10, hitung 12 | Stok 12 |
| INT-07 | Aktor 99 | Ditolak |
| INT-08 | Admin, jumlah 7 | Approved otomatis |
| INT-09 | Produk dan stok | Tidak tampil |
| INT-10 | Tambah/ubah/hapus kategori | Tiga catatan audit |

**Ringkasan hasil:**

| Metrik | Nilai |
|---|---:|
| Total test | 171 |
| Passed | 171 |
| Failed | 0 |
| Cakupan baris | 74,69% |
| Cakupan cabang | 60,17% |

## 5.4 Catat analisis hasil pengujian sesuai standar dokumentasi

| Standar | Status |
|---|---|
| Hasil yang diharapkan dan nyata tercatat | Ya |
| Setiap test terisolasi | Ya |
| Hasil dapat diulang | Ya |
| Cakupan kode dilaporkan | Ya |
| Regresi dijalankan | 171 lulus |
| Integrasi PostgreSQL | Belum |

**Kesimpulan:** seluruh skenario integrasi inti berhasil dan terdokumentasi.
Perlu pengujian tambahan dengan PostgreSQL nyata dan lapisan API/UI.

## 5.5 Hasil Pengujian Integrasi Third Party

Pengujian integrasi dengan pihak ketiga dilakukan melalui **HTTP stub lokal**
(`ThirdPartyApprovalStub`) yang mensimulasikan kontrak API approval vendor.
Hasil dicatat terpisah dari pengujian integrasi internal pada
`sheets/result-sheet-third-party.csv`.

| Test ID | Endpoint | Auth | Expected | Actual | Status Code | Stock | Status |
|---|---|---|---|---|---|---|---|
| EXT-11 | `POST /api/approvals` | valid key | Approved; stock 7 | Approved; stock 7; 1 movement In | 200 | 7 | Pass |
| EXT-12 | `POST /api/approvals` | valid key | Rejected; stock 0 | Rejected; stock 0 | 200 | 0 | Pass |
| EXT-13 | `POST /api/approvals` | invalid key | 401; stock 0 | 401; callback tidak jalan | 401 | 0 | Pass |
| EXT-14 | `POST /api/approvals` | valid key | 500; stock 0 | 500; stock 0 | 500 | 0 | Pass |
| EXT-15 | `POST /api/approvals` | valid key | Callback kedua 409; stock 7 | Callback pertama 200; kedua 409; stock 7 | 200/409 | 7 | Pass |

**Ringkasan third party:**

| Metrik | Nilai |
|---|---:|
| Total skenario | 5 |
| Passed | 5 |
| Failed | 0 |
| Transport | HTTP stub lokal (`127.0.0.1`) |
| Autentikasi | Header `X-Api-Key` |
