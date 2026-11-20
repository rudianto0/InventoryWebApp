# 1. Buatlah Berbagai Operasi terhadap Basis Data

## a. Simpan/ubah data ke dalam format basis data.

Aplikasi web menyimpan data dari form ke database. Contohnya saat pengguna
login, menambah produk, atau mencatat stok masuk dan keluar.

| Kegiatan | Contoh | Perintah database |
|---|---|---|
| Tambah data | Produk baru, stok masuk | `INSERT` |
| Ubah data | Ganti nama produk, setujui transaksi | `UPDATE` |
| Hapus data | Hapus produk (soft delete) | `UPDATE IsDeleted = true` |
| Login/logout | Cek user dan peran | `SELECT` user dan role |

```csharp
var product = new Product { Sku = "SKU-010", Name = "Notebook" };
db.Products.Add(product);
await db.SaveChangesAsync();
```

Padanan SQL:

```sql
INSERT INTO "Products" ("Id", "Sku", "Name", "IsDeleted")
VALUES (gen_random_uuid(), 'SKU-010', 'Notebook', false);
```

Setiap perubahan otomatis dicatat waktunya dan siapa yang mengubah, sehingga
riwayat tetap terlacak.

## b. Hasilkan informasi yang diinginkan menggunakan query tersebut.

Query dipakai untuk menampilkan informasi yang dibutuhkan pengguna.

| Informasi | Sumber data |
|---|---|
| Total stok per barang/lokasi | `Stocks`, `Products`, `Locations` |
| Daftar transaksi | header/detail transaksi |
| Barang perlu dibeli ulang | `v_reorder_alert` |
| Menunggu persetujuan | `v_pending_approvals` |
| Kartu stok | `fn_stock_card` |

```sql
SELECT p."Sku", p."Name", s."Quantity"
FROM "Stocks" s
JOIN "Products" p ON p."Id" = s."ProductId"
WHERE s."LocationId" = '<location-id>';
```

## c. Gunakan indeks untuk mempercepat akses.

Indeks seperti daftar isi buku: mempercepat pencarian tanpa membaca semua
halaman.

| Indeks | Mempercepat |
|---|---|
| Produk berdasarkan SKU | Pencarian produk |
| Stok per produk dan lokasi | Cek saldo |
| Pergerakan per produk dan tanggal | Kartu stok dan laporan |
| Tanggal/status transaksi | Daftar dan approval |

```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM "StockMovements"
WHERE "ProductId" = '<product-id>'
ORDER BY "CreatedAt" DESC
LIMIT 50;
```

Jika hasilnya `Index Scan`, indeks sudah dipakai. Jika `Seq Scan` pada tabel
besar, perlu ditambah indeks atau filter yang lebih tepat.
