# 03 - Indeks dan View

## 1. Indeks

Script: `../sql/03-index-and-view/03-indexes.sql`

### Indeks tambahan

| Tabel | Indeks | Tujuan |
|---|---|---|
| `StockMovements` | `(LocationId, CreatedAt DESC)` | Laporan per lokasi |
| `StockMovements` | `(ProductId, CreatedAt DESC)` | Kartu stok |
| `StockMovements` | `(ReferenceType, ReferenceId)` | Trace dokumen |
| Header transaksi | `(Date DESC)` | Filter tanggal |
| Header transaksi | `(ApproverId)` partial | Pending approval |
| `AuditLogs` | `(EntityName, Action, CreatedAt DESC)` | Filter audit |
| `Products`, `Suppliers`, `Locations` | partial `(IsActive)` | Opsi master |

Semua memakai `CREATE INDEX IF NOT EXISTS` agar aman dijalankan ulang.

### Catatan efisiensi

- Index partial `WHERE "IsDeleted" = false` mengurangi ukuran index.
- Index tanggal menurun membantu query `ORDER BY ... DESC`.
- Hindari index berlebihan pada tabel write-heavy; fokus pada pola query nyata.

## 2. View

Script: `../sql/03-index-and-view/04-views.sql`

| View | Jenis | Isi |
|---|---|---|
| `v_product_basic` | Updatable | Produk aktif (view sederhana) |
| `v_stock_balance` | Read-only | Saldo per produk/lokasi + reorder |
| `v_reorder_alert` | Read-only | Stok ≤ reorder level |
| `v_pending_approvals` | Read-only | Union 5 tipe transaksi status Submitted |
| `v_stock_movement_daily` | Read-only | Agregasi mutasi per hari |
| `v_stock_card` | Read-only | Movement + running balance |
| `v_audit_summary` | Read-only | Rekap audit per entitas/aksi |

### Contoh penggunaan

```sql
SELECT * FROM "v_stock_balance" ORDER BY "Sku";

SELECT * FROM "v_reorder_alert"
ORDER BY "Quantity";

SELECT * FROM "v_pending_approvals"
ORDER BY "SubmittedAt";

SELECT * FROM "v_stock_card"
WHERE "Sku" = 'SKU-001'
ORDER BY "CreatedAt";
```

### Updatable view

`v_product_basic` hanya mengambil satu tabel tanpa GROUP BY/DISTINCT, sehingga
PostgreSQL mengizinkan `INSERT`, `UPDATE`, dan `DELETE` langsung. View lain yang
melibatkan JOIN/agregasi bersifat read-only.

## 3. Verifikasi

```sql
SELECT table_name
FROM information_schema.views
WHERE table_schema = 'public'
ORDER BY table_name;
```
