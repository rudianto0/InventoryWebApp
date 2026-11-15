# 08 - Commit, Rollback, dan Savepoint

Script: `sql/postgresql/09-transactions.sql`

## 1. COMMIT

```sql
BEGIN;
INSERT INTO "Suppliers" (...) VALUES ('SUP-TRX-COMMIT', ...);
COMMIT;

SELECT COUNT(*) FROM "Suppliers" WHERE "Code" = 'SUP-TRX-COMMIT';
```

Hasil: data tersimpan (count = 1).

## 2. ROLLBACK

```sql
BEGIN;
INSERT INTO "Suppliers" (...) VALUES ('SUP-TRX-ROLLBACK', ...);
ROLLBACK;

SELECT COUNT(*) FROM "Suppliers" WHERE "Code" = 'SUP-TRX-ROLLBACK';
```

Hasil: data dibatalkan (count = 0).

## 3. SAVEPOINT

```sql
BEGIN;
INSERT ...;                    -- SUP-TRX-SAVE1
SAVEPOINT before_second;
INSERT ...;                    -- SUP-TRX-SAVE2
ROLLBACK TO SAVEPOINT before_second;
COMMIT;
```

Hasil: hanya `SUP-TRX-SAVE1` tersimpan.

## 4. Uji Procedure, Function, Trigger

Script yang sama juga menjalankan:

- `sp_receive_stock`, `sp_issue_stock`, `sp_transfer_stock`.
- `sp_approve_stock_in` pada header Submitted.
- `sp_recalculate_stock`.
- Function saldo, nomor dokumen, pending count, stock balance, stock card.
- Trigger validasi dan kalkulasi.

## 5. Verifikasi Akhir

```sql
SELECT
    (SELECT COUNT(*) FROM "Products") AS "Products",
    (SELECT COUNT(*) FROM "Stocks") AS "Stocks",
    (SELECT COUNT(*) FROM "StockMovements") AS "Movements",
    (SELECT COUNT(*) FROM "v_pending_approvals") AS "PendingApprovals";
```

## 6. Catatan DBeaver

- Gunakan **SQL Editor → Execute SQL Script** agar urutan `BEGIN`/`COMMIT`
  dieksekusi dalam satu sesi.
- Jika DBeaver dalam mode auto-commit, transaksi eksplisit tetap dihormati.
- Untuk mengulang demonstrasi, jalankan ulang script; data uji akan bertambah
  sesuai desain.
