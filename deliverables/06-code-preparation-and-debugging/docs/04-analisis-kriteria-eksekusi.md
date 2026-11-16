# 04 - Analisis Kriteria Eksekusi Aplikasi

## 1. Perintah Menjalankan

```powershell
dotnet run --project src/Inventory.Web
```

Atau F5 dari IDE.

## 2. Kriteria Aplikasi Berhasil Dieksekusi

| No | Kriteria | Cara Cek |
|---|---|---|
| 1 | Proses start tanpa exception | Console/log |
| 2 | Kestrel/IIS Express listening | URL log |
| 3 | Halaman login terbuka | Browser |
| 4 | Login berhasil | Akun default |
| 5 | Cookie auth aktif | DevTools |
| 6 | Database terhubung | Dashboard/list data |
| 7 | Migrasi & seeding selesai | Log `Database initialized` |
| 8 | Halaman dashboard tampil | Browser |
| 9 | CRUD master berjalan | Uji tambah/edit/hapus |
| 10 | Transaksi stok berjalan | Stock In/Out + approve |
| 11 | Laporan menampilkan data | Stock Balance |

## 3. Verifikasi Database

```sql
SELECT COUNT(*) FROM "__EFMigrationsHistory";
SELECT COUNT(*) FROM "Products";
SELECT COUNT(*) FROM "Stocks";
SELECT * FROM "v_stock_balance" ORDER BY "Sku";
```

## 4. Verifikasi Autentikasi

- Buka `/Account/Login`.
- Login dengan akun hasil seeding.
- Pastikan redirect ke Dashboard.
- Logout dan pastikan kembali ke login.

## 5. Verifikasi Transaksi

1. Buat Stock In (Draft).
2. Submit.
3. Approve.
4. Cek `Stocks` bertambah dan `StockMovements` tercatat.
5. Ulangi untuk Stock Out/Transfer/Adjustment/Opname.

## 6. Gejala Gagal Eksekusi

| Gejala | Kemungkinan | Solusi |
|---|---|---|
| Port sudah dipakai | Proses lain | Stop proses/ganti port |
| DB connection refused | PostgreSQL mati/salah host | Cek service/config |
| Login gagal | User belum di-seed | Jalankan bootstrap/seeding |
| 500 error | Exception runtime | Lihat log/stack trace |
| Halaman kosong | Razor/model error | Cek log & view |
| Migrasi error | Schema tidak cocok | Backup, perbaiki migration |

## 7. Kriteria Lulus

Aplikasi dinyatakan berhasil jika:

- start tanpa exception,
- dapat login,
- data master dan transaksi dapat diakses,
- database konsisten,
- tidak ada error 500 pada alur utama.
