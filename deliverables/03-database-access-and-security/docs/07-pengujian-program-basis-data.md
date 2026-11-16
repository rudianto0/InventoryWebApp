# 07 - Pengujian Program Basis Data

## 7a. Skenario Pengujian

| No | Skenario | Langkah | Ekspektasi |
|---|---|---|---|
| 1 | Insert master | Insert kategori/produk | Data tersimpan, audit terisi |
| 2 | Update master | Ubah nama produk | Data berubah, `UpdatedAt` terisi |
| 3 | Soft delete | Hapus produk | Hilang dari query, `IsDeleted=true` |
| 4 | Stock In | Posting + approve | Saldo naik, movement `In` tercatat |
| 5 | Stock Out | Posting melebihi saldo | Ditolak, saldo tidak berubah |
| 6 | Transfer | Transfer antar lokasi | Saldo asal turun, tujuan naik |
| 7 | Adjustment | Counted ≠ system | `Difference` benar, saldo sesuai |
| 8 | Opname | Submit/approve | Saldo = counted, movement `Opname` |
| 9 | Approval | Approve oleh approver | Status Approved + stok diterapkan |
| 10 | Report | Buka stock balance/card | Data benar, response < 2 detik |
| 11 | Index | Query dengan/ tanpa index | Index Scan vs Seq Scan terlihat |
| 12 | Hak akses | `inventory_app` DDL | Ditolak |
| 13 | Koneksi TLS | Koneksi produksi | Terenkripsi |
| 14 | Konkurensi | Dua update stok bersamaan | Konflik terdeteksi/retry |
| 15 | N+1 | Buka approval 100 item | Query tidak meledak setelah optimasi |
| 16 | Export | Export CSV | Data lengkap/terstream |

## 7b. Membaca Logika dan Kinerja Statement

### EF Core logging

```csharp
optionsBuilder
    .UseNpgsql(connection)
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
```

`EnableSensitiveDataLogging` hanya untuk development.

### Query tag

Tambahkan penanda agar mudah dilacak:

```csharp
db.Stocks.TagWith("Report.StockBalance").AsNoTracking()...
```

### Deteksi N+1

- Aktifkan logging EF Core.
- Buka halaman approval dengan banyak item.
- Hitung jumlah query.
- Perbaiki dengan join/projection/batch.

Contoh temuan: `ApprovalService.GetPendingAsync` melakukan query creator per item.

### PostgreSQL statistics

```sql
-- query paling mahal
SELECT query, calls, total_exec_time, mean_exec_time, rows
FROM pg_stat_statements
ORDER BY total_exec_time DESC
LIMIT 20;

-- aktivitas saat ini
SELECT pid, usename, state, wait_event_type, wait_event, query
FROM pg_stat_activity
WHERE datname = 'InventoryAppDB';

-- ukuran tabel/index
SELECT relname, pg_size_pretty(pg_total_relation_size(relid)) AS total
FROM pg_catalog.pg_statio_user_tables
ORDER BY pg_total_relation_size(relid) DESC;
```

### Execution plan

```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM "v_stock_card" ORDER BY "CreatedAt" DESC LIMIT 50;
```

Yang dinilai:

- `Seq Scan` pada tabel besar.
- `Nested Loop` berlebihan.
- `Sort` dengan disk spill.
- `Buffers: shared read` tinggi.
- Estimasi vs actual rows jauh berbeda (statistik usang).

## 7c. Uji Performansi

### Dataset uji

| Data | Jumlah |
|---|---|
| Produk | 10.000 |
| Stok | 20.000 baris |
| StockMovements | 100.000 baris |
| Header transaksi | 20.000 |
| AuditLogs | 100.000 |

### Skenario beban

1. Buka dashboard + stock balance (read).
2. Buka daftar transaksi dengan filter tanggal.
3. Buka kartu stok 1 tahun.
4. Submit 20 transaksi bersamaan pada produk populer.
5. Approve 50 dokumen pending.
6. Export CSV 10.000 baris.

### Metrik

| Metrik | Target awal |
|---|---|
| p50 list | < 200 ms |
| p95 list | < 500 ms |
| p95 report | < 2.000 ms |
| Error rate | < 1% |
| DB CPU | < 70% |
| Buffer hit ratio | > 95% |
| Lock wait | minimal |

### Alat

- `EXPLAIN (ANALYZE, BUFFERS)` untuk query individual.
- `pg_stat_statements` untuk query terburuk.
- `pgbench` untuk beban DML langsung ke DB (dokumentasi saja).
- k6/NBomber untuk endpoint aplikasi.
- Monitoring OS: CPU, RAM, disk I/O.

### Langkah

1. Jalankan baseline **tanpa index tambahan** (drop index di DB uji).
2. Catat waktu dan plan.
3. Tambah index (`03-indexes.sql`).
4. Jalankan ulang query dan beban yang sama.
5. Bandingkan p50/p95 dan plan.
6. Simpan hasil di dokumen/laporan.

### Contoh kesimpulan yang diharapkan

- Query kartu stok memakai `Index Scan` setelah index `(ProductId, CreatedAt)`.
- Laporan movement lebih cepat dengan filter tanggal + index.
- Approval setelah batch query turun dari O(n) query menjadi O(1)–O(k).
