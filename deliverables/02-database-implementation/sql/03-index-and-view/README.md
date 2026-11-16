# 02.03 - Index dan View

## Konteks

Menambah indeks untuk laporan/pencarian dan membuat view operasional.

## File

| File | Fungsi |
|---|---|
| `03-indexes.sql` | Index tambahan (movement, header, audit, partial active) |
| `04-views.sql` | `v_stock_balance`, `v_reorder_alert`, `v_pending_approvals`, `v_stock_movement_daily`, `v_stock_card`, `v_audit_summary`, `v_product_basic` |

## Prasyarat

Schema + seed data.

## Verifikasi

```sql
SELECT table_name FROM information_schema.views
WHERE table_schema = 'public' ORDER BY table_name;

SELECT * FROM "v_stock_balance" ORDER BY "Sku";
```
