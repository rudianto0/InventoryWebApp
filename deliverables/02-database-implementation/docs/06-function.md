# 06 - Function

Script: `../sql/06-function/07-functions.sql`

## Daftar Function

| Function | Return | Fungsi |
|---|---|---|
| `fn_available_stock(product, location)` | `integer` | Saldo tersedia |
| `fn_next_document_number(prefix, date)` | `text` | Nomor dokumen atomic |
| `fn_pending_approval_count(user_id)` | `integer` | Jumlah pending approval |
| `fn_stock_balance(product, location)` | `TABLE` | Saldo + reorder |
| `fn_stock_card(product, from, to)` | `TABLE` | Kartu stok + running balance |

## Detail

### fn_available_stock

```sql
SELECT fn_available_stock(
    '50000000-0000-0000-0000-000000000001',
    '40000000-0000-0000-0000-000000000002'
);
```

Mengembalikan 0 jika baris stok belum ada.

### fn_next_document_number

```sql
SELECT fn_next_document_number('IN', CURRENT_DATE);
```

Contoh hasil: `IN-202611-0003`.

Menggunakan `INSERT ... ON CONFLICT DO UPDATE ... RETURNING` sehingga increment
atomic dan aman untuk concurrent request.

### fn_pending_approval_count

```sql
SELECT fn_pending_approval_count(NULL); -- semua
SELECT fn_pending_approval_count(2);    -- approver tertentu
```

### fn_stock_balance

```sql
SELECT * FROM fn_stock_balance(
    '50000000-0000-0000-0000-000000000001',
    '40000000-0000-0000-0000-000000000002'
);
```

### fn_stock_card

```sql
SELECT * FROM fn_stock_card(
    '50000000-0000-0000-0000-000000000001',
    CURRENT_DATE - 30,
    CURRENT_DATE
);
```

Menghitung `RunningBalance` dengan window function, lalu memfilter rentang
tanggal.

## Efisiensi

- `LANGUAGE sql STABLE` untuk read-only agar optimizer dapat mengoptimalkan.
- `plpgsql` hanya untuk logika yang membutuhkan variabel/exception.
- Index pada `StockMovements (ProductId, CreatedAt)` mendukung kartu stok.
- Function nomor dokumen menghindari race condition read-modify-write.
