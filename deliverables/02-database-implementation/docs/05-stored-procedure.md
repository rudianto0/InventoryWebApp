# 5. Membuat Stored Procedure

## 5.1 Buat stored procedure dengan perintah SQL

Stored procedure adalah program yang disimpan di database untuk menjalankan
tugas berulang, misalnya mencatat stok masuk dan mengubah saldo sekaligus.

| Procedure | Fungsi |
|---|---|
| `sp_receive_stock` | Mencatat barang masuk |
| `sp_issue_stock` | Mencatat barang keluar |
| `sp_transfer_stock` | Memindahkan stok antar lokasi |
| `sp_approve_stock_in` | Menyetujui penerimaan |
| `sp_recalculate_stock` | Menghitung ulang saldo stok |

```sql
CREATE OR REPLACE PROCEDURE sp_receive_stock(
    INOUT p_doc_id uuid,
    IN p_supplier_id uuid,
    IN p_location_id uuid,
    IN p_product_id uuid,
    IN p_quantity integer,
    IN p_user_id integer
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- validasi, simpan dokumen, tambah stok, catat pergerakan
END;
$$;
```

Untuk master data, pengelolaan dilakukan melalui perintah DML dan view
`v_product_basic` karena operasinya sederhana.

## 5.2 Uji dan periksa prosedur input dan outputnya

```sql
CALL sp_receive_stock(
    NULL,
    '30000000-0000-0000-0000-000000000001',
    '40000000-0000-0000-0000-000000000002',
    '50000000-0000-0000-0000-000000000001',
    5,
    1
);
```

Periksa hasilnya dengan `SELECT * FROM "v_stock_balance";`. Jika stok bertambah,
prosedur berjalan benar.
