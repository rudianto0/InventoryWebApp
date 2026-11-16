# 02 - Dokumentasi Uji Coba

## 1. Kebutuhan Dokumentasi

| Dokumen | Isi |
|---|---|
| Test plan | Tujuan, scope, tools, jadwal |
| Test scenario | Daftar skenario |
| Test case | Langkah, data, expected |
| Test data | Data valid/invalid/boundary |
| Test result | Pass/fail + bukti |
| Defect log | Error + perbaikan |
| Coverage report | Line/branch |

## 2. Template Test Case

| Field | Keterangan |
|---|---|
| ID | `TC-<MODUL>-NNN` |
| Modul | `MOD-*` |
| Judul | Ringkas |
| Precondition | Data/user |
| Steps | Langkah |
| Expected | Hasil |
| Actual | Hasil nyata |
| Status | Pass/Fail |
| Evidence | Log/TRX |

## 3. Variasi Kondisi

| Variasi | Contoh |
|---|---|
| Data valid | master lengkap, qty > 0 |
| Boundary | qty 0/1, page size 1/200 |
| Invalid | string kosong, negatif, duplikat |
| State | aktif/nonaktif/soft-deleted |
| Authorization | Staff/SPV/Admin/parent invalid |
| Concurrency | dua update stok |
| Empty | tanpa stok/produk |
| Error DB | constraint/concurrency |

## 4. Struktur Dokumentasi

```
Test Plan
 ├─ Scope
 ├─ Tools
 ├─ Data
 ├─ Skenario
 ├─ Eksekusi
 └─ Hasil & Analisis
```

## 5. Aturan

- Setiap test harus dapat direproduksi.
- Setiap bug harus punya test regresi.
- Hasil disimpan di `results/`.
- Coverage diperbarui setiap perubahan.

## 6. Traceability

| Requirement | Test |
|---|---|
| Approval 1 level | `Stock*ServiceTests`, `ApprovalServiceTests` |
| Stok tidak negatif | `StockLedgerTests` |
| Validasi master | `CatalogServiceTests`, `SupplierAndLocationTests` |
| Audit/soft delete | `SoftDeleteTests`, `AuditTrailExtraTests` |
| User/role | `UserServiceTests` |
