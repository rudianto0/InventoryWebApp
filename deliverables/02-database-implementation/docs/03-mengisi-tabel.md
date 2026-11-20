# 3. Mengisi Tabel

## 3.1 Isi data tabel menggunakan perintah DML

DML adalah perintah untuk mengisi dan mengubah data (`INSERT`, `UPDATE`,
`DELETE`, `SELECT`). Data contoh diisi berurutan agar relasi tidak error:
master, lokasi, produk, stok, transaksi, lalu pergerakan stok.

```sql
INSERT INTO "Categories" ("Id", "Name", "IsActive", "IsDeleted")
VALUES (gen_random_uuid(), 'General', true, false);

INSERT INTO "Products" ("Id", "Sku", "Name", "CategoryId", "UnitId", "ReorderLevel")
VALUES (gen_random_uuid(), 'SKU-001', 'Kertas A4', '<category-id>', '<unit-id>', 10);
```

Data yang tersedia: kategori, unit, supplier, lokasi, produk, stok, lima jenis
transaksi, pergerakan stok, nomor dokumen, dan contoh audit.

## 3.2 Buat indeks

Indeks mempercepat pencarian dan laporan. Contoh:

```sql
CREATE INDEX IF NOT EXISTS "IX_StockMovements_ProductId_CreatedAt"
ON "StockMovements" ("ProductId", "CreatedAt" DESC);
```

Indeks penting: pergerakan per produk dan lokasi, tanggal transaksi, status
persetujuan, dan filter audit.

## 3.3 Buat view tabel sesuai kebutuhan

View adalah tampilan siap pakai dari data yang digabungkan. View yang tersedia:

| View | Isi |
|---|---|
| `v_product_basic` | Data produk (bisa diubah langsung) |
| `v_stock_balance` | Saldo stok per barang dan lokasi |
| `v_reorder_alert` | Barang yang perlu dibeli ulang |
| `v_pending_approvals` | Transaksi menunggu persetujuan |
| `v_stock_movement_daily` | Ringkasan pergerakan per hari |
| `v_stock_card` | Kartu stok dan saldo berjalan |
| `v_audit_summary` | Ringkasan riwayat perubahan |

```sql
SELECT * FROM "v_stock_balance" ORDER BY "Sku";
```
