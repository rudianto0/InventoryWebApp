# 05 - Stored Procedure

Script: `sql/postgresql/06-stored-procedures.sql`

## Daftar Procedure

| Procedure | Parameter | Fungsi |
|---|---|---|
| `sp_receive_stock` | `INOUT doc_id`, supplier, location, product, qty, user | Posting penerimaan + update stok + movement |
| `sp_issue_stock` | `INOUT doc_id`, location, product, qty, user | Validasi saldo + pengeluaran + movement |
| `sp_transfer_stock` | `INOUT doc_id`, from, to, product, qty, user | Transfer + dua movement |
| `sp_approve_stock_in` | header_id, approver | Approve header Submitted + terapkan stok |
| `sp_recalculate_stock` | location, `INOUT rows` | Hitung ulang saldo dari movement |

## Detail

### sp_receive_stock

1. Validasi `qty > 0`.
2. Ambil nomor dokumen atomic dari `DocumentSequences`.
3. Insert header status Approved dan detail.
4. Upsert `Stocks` (`ON CONFLICT (ProductId, LocationId)`).
5. Insert `StockMovements` tipe In.

### sp_issue_stock

1. Validasi qty.
2. `UPDATE Stocks ... WHERE Quantity >= qty`.
3. Jika tidak ada baris terupdate → raise exception.
4. Insert header/detail dan movement negatif.

### sp_transfer_stock

1. Validasi qty dan lokasi asal ≠ tujuan.
2. Kurangi stok asal dengan validasi.
3. Upsert stok tujuan.
4. Insert dua movement (`TransferOut`, `TransferIn`).

### sp_approve_stock_in

1. Lock header `FOR UPDATE`.
2. Hanya status Submitted (1) yang diproses.
3. Loop detail → update stok + movement.
4. Update status menjadi Approved (2).

### sp_recalculate_stock

Menghitung `SUM(Quantity)` dari movement per produk di lokasi lalu upsert ke
`Stocks`. Berguna untuk rekonsiliasi.

## Cara Menguji

```sql
CALL sp_receive_stock(
    NULL,
    '30000000-0000-0000-0000-000000000001',
    '40000000-0000-0000-0000-000000000002',
    '50000000-0000-0000-0000-000000000001',
    5,
    1
);

CALL sp_issue_stock(
    NULL,
    '40000000-0000-0000-0000-000000000002',
    '50000000-0000-0000-0000-000000000001',
    3,
    1
);

CALL sp_transfer_stock(
    NULL,
    '40000000-0000-0000-0000-000000000002',
    '40000000-0000-0000-0000-000000000004',
    '50000000-0000-0000-0000-000000000001',
    2,
    1
);
```

Verifikasi:

```sql
SELECT * FROM "v_stock_balance" ORDER BY "Sku";
SELECT * FROM "v_pending_approvals";
```

## Catatan

- Procedure mengubah data; jalankan pada database uji.
- `sp_receive_stock` langsung Approved agar sederhana untuk demonstrasi.
- Untuk alur aplikasi penuh (Draft → Submit → Approve), gunakan
  `sp_approve_stock_in`.
