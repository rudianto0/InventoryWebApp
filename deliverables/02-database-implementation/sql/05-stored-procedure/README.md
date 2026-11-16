# 02.05 - Stored Procedure

## Konteks

Procedure PostgreSQL untuk alur transaksi stok.

## File

- `06-stored-procedures.sql`

## Procedure

| Procedure | Fungsi |
|---|---|
| `sp_receive_stock` | Penerimaan + update stok + movement |
| `sp_issue_stock` | Pengeluaran + validasi saldo |
| `sp_transfer_stock` | Transfer + dua movement |
| `sp_approve_stock_in` | Approve header Submitted |
| `sp_recalculate_stock` | Rekalkulasi saldo dari movement |

## Prasyarat

Schema, seed, dan view.

## Verifikasi

```sql
CALL sp_receive_stock(NULL, '<supplier>', '<location>', '<product>', 5, 1);
SELECT * FROM "v_stock_balance" ORDER BY "Sku";
```

## Catatan

Uji procedure mengubah data; gunakan database uji.
