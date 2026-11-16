# 02.06 - Function

## Konteks

Function PostgreSQL untuk perhitungan dan lookup.

## File

- `07-functions.sql`

## Function

| Function | Return | Fungsi |
|---|---|---|
| `fn_available_stock` | integer | Saldo tersedia |
| `fn_next_document_number` | text | Nomor dokumen atomic |
| `fn_pending_approval_count` | integer | Jumlah pending |
| `fn_stock_balance` | table | Saldo per produk/lokasi |
| `fn_stock_card` | table | Kartu stok + running balance |

## Verifikasi

```sql
SELECT fn_available_stock('<product>', '<location>');
SELECT fn_next_document_number('IN', CURRENT_DATE);
SELECT * FROM fn_stock_card('<product>', CURRENT_DATE - 30, CURRENT_DATE);
```
