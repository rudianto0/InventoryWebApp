# 03 - Data Uji Integrasi

## 1. Dataset Dasar

| Data | Nilai |
|---|---|
| Kategori | General |
| Unit | PCS |
| Produk | SKU-1 / Product 1 |
| Lokasi | HQ (Branch), WH1, WH2 (Warehouse) |
| Supplier | SUP |
| Stok awal | WH1=10, WH2=0 |
| User | Staff=10, SPV=20, Admin=1 |
| Password test | `Password1!` (fake) |

## 2. Data per Skenario

| TestID | Input | Expected State |
|---|---|---|
| INT-01 | qty 7, approver 20 | stok 7; status Approved |
| INT-02 | stok 10, qty 4 | stok 6; movement -4 |
| INT-03 | stok 2, qty 5 | error; stok 2 |
| INT-04 | WH1 10, qty 4 | WH1 6; WH2 4 |
| INT-05 | system 10, counted 7 | difference -3; stok 7 |
| INT-06 | system 10, counted 12 | stok 12 |
| INT-07 | creator 10, actor 99 | error; stok tetap |
| INT-08 | admin, qty 7 | approved otomatis |
| INT-09 | product + stock | soft delete; hidden |
| INT-10 | category CRUD | AuditLogs 3 aksi |

## 3. Generate Data

- Fixture `TestDatabase` membuat SQLite in-memory.
- `SeedBasicsAsync` membuat data dasar.
- `SeedStockAsync` membuat saldo.
- `IdentityFixture` membuat user/role.
- Fake directory/document number untuk determinisme.

## 4. Variasi Kondisi

| Kondisi | Contoh |
|---|---|
| Sukses | stok cukup, approver valid |
| Gagal validasi | stok kurang, qty negatif |
| Otorisasi | non-approver, self-approve |
| State | Draft/Submitted/Approved |
| Soft delete | product/location deleted |
| Admin | auto-approve |

## 5. Aturan

- Tidak memakai data produksi/PII.
- Setiap test terisolasi.
- Data dibersihkan setelah test.
- Nilai waktu UTC.
