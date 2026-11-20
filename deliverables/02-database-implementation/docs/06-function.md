# 6. Membuat Function

## 6.1 Buat function dengan perintah SQL

Function adalah rumus atau perhitungan siap pakai yang disimpan di database.

| Function | Hasil |
|---|---|
| `fn_available_stock` | Jumlah stok tersedia |
| `fn_next_document_number` | Nomor dokumen berikutnya |
| `fn_pending_approval_count` | Jumlah transaksi menunggu |
| `fn_stock_balance` | Saldo stok dan batas pembelian ulang |
| `fn_stock_card` | Kartu stok dengan saldo berjalan |

```sql
SELECT fn_available_stock(
    '50000000-0000-0000-0000-000000000001',
    '40000000-0000-0000-0000-000000000002'
);
```

Belum ada function khusus rekap bulanan. Rekap stok masuk dan keluar dapat
dihitung dari `fn_stock_card` atau `v_stock_movement_daily` dengan filter
tanggal/bulan. Jika rekap bulanan perlu menjadi satu function tersendiri,
function tersebut belum dibuat.

## 6.2 Tulis perintah SQL pada function secara efisien

- Gunakan `LANGUAGE sql STABLE` untuk function yang hanya membaca data.
- Dukung dengan indeks pada kolom tanggal dan produk.
- Gunakan `INSERT ... ON CONFLICT ... RETURNING` untuk nomor dokumen agar aman
  saat banyak pengguna memakai bersamaan.
