# 4. Melakukan Operasi Relasional

## 4.1 Identifikasi fitur pengolahan DML

Fitur yang dipakai: `JOIN` untuk menggabungkan tabel, subquery, CTE,
`INSERT ... SELECT`, `UPDATE ... FROM`, dan `DELETE ... USING`.

## 4.2 Gunakan perintah DML untuk manipulasi antar tabel

Contoh menggabungkan produk, kategori, dan stok:

```sql
SELECT p."Sku", p."Name", c."Name" AS "Category", s."Quantity"
FROM "Products" p
JOIN "Categories" c ON c."Id" = p."CategoryId"
LEFT JOIN "Stocks" s ON s."ProductId" = p."Id";
```

## 4.3 Gunakan perintah DML untuk manipulasi antar view

View sederhana `v_product_basic` dapat diisi, diubah, dan dihapus langsung.
View yang menggabungkan banyak tabel hanya untuk membaca data.

```sql
INSERT INTO "v_product_basic" ("Id", "Sku", "Name", "CategoryId", "UnitId")
VALUES (gen_random_uuid(), 'SKU-VIEW-01', 'Produk via View', '<category-id>', '<unit-id>');

DELETE FROM "v_product_basic" WHERE "Sku" = 'SKU-VIEW-01';
```

## 4.4 Tulis perintah DML secara efisien

- Ambil kolom yang diperlukan dan hindari `SELECT *`.
- Gunakan `EXISTS` untuk pengecekan data.
- Batasi laporan dengan filter tanggal.
- Gunakan `EXPLAIN ANALYZE` untuk melihat query yang lambat.
