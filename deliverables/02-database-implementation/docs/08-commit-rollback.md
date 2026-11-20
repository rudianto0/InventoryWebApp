# 8. Melakukan Perintah Commit dan Rollback

## 8.1 Lakukan perubahan data dengan perintah commit

`COMMIT` menyimpan perubahan secara permanen.

```sql
BEGIN;
INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "IsDeleted")
VALUES (gen_random_uuid(), 'SUP-TRX-COMMIT', 'Supplier Commit', true, false);
COMMIT;

SELECT COUNT(*) FROM "Suppliers" WHERE "Code" = 'SUP-TRX-COMMIT';
```

Hasil: data tersimpan (jumlah 1). Pada contoh "tarif", nilai harga atau biaya
satuan (`UnitCost`) disimpan dengan cara yang sama.

## 8.2 Lakukan pembatalan penulisan data dengan rollback

`ROLLBACK` membatalkan perubahan yang belum disimpan.

```sql
BEGIN;
INSERT INTO "Suppliers" ("Id", "Code", "Name", "IsActive", "IsDeleted")
VALUES (gen_random_uuid(), 'SUP-TRX-ROLLBACK', 'Supplier Rollback', true, false);
ROLLBACK;

SELECT COUNT(*) FROM "Suppliers" WHERE "Code" = 'SUP-TRX-ROLLBACK';
```

Hasil: data batal tersimpan (jumlah 0). Script aktual di
`sql/08-transaction/09-transactions.sql` memakai pola INSERT lalu ROLLBACK.
Pada contoh "pelanggan", data pelanggan dipadankan dengan data supplier.

Untuk demonstrasi rollback setelah menghapus satu data, pola `BEGIN; DELETE ...;
ROLLBACK;` juga valid dan menghasilkan data tetap ada. Script utama memakai
INSERT agar data uji lain tidak ikut terhapus.
