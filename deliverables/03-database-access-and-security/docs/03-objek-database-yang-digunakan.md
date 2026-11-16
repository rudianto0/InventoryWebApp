# 03 - Objek Database yang Digunakan

## 3.1 Tabel

| Kelompok | Tabel |
|---|---|
| Master | `Categories`, `Units`, `Products`, `Suppliers`, `Locations` |
| Persediaan | `Stocks`, `StockMovements` |
| Transaksi | `StockInHeaders/Details`, `StockOutHeaders/Details`, `StockTransferHeaders/Details`, `StockAdjustmentHeaders/Details`, `StockOpnameHeaders/Details` |
| Sistem | `DocumentSequences`, `AuditLogs` |
| Identity | `Users`, `Roles`, `UserRoles`, `UserClaims`, `UserLogins`, `RoleClaims`, `UserTokens` |

## 3.2 View

| View | Dipakai untuk |
|---|---|
| `v_stock_balance` | Saldo stok per produk/lokasi |
| `v_reorder_alert` | Produk di bawah reorder level |
| `v_pending_approvals` | Daftar approval |
| `v_stock_movement_daily` | Rekap mutasi harian |
| `v_stock_card` | Kartu stok + running balance |
| `v_audit_summary` | Rekap audit |
| `v_product_basic` | Contoh view updatable |

## 3.3 Function dan Procedure

| Objek | Fungsi |
|---|---|
| `fn_available_stock` | Cek saldo tersedia |
| `fn_next_document_number` | Nomor dokumen atomic |
| `fn_pending_approval_count` | Jumlah pending |
| `fn_stock_balance` | Saldo per produk/lokasi |
| `fn_stock_card` | Kartu stok |
| `sp_receive_stock` | Penerimaan stok |
| `sp_issue_stock` | Pengeluaran stok |
| `sp_transfer_stock` | Transfer stok |
| `sp_approve_stock_in` | Approve Stock In |
| `sp_recalculate_stock` | Rekalkulasi saldo |

## 3.4 Index Utama

| Tabel | Index |
|---|---|
| `Products` | `Sku` unique, `CategoryId` |
| `Locations` | `Code` unique, `ParentLocationId`, `BranchCode` |
| `Stocks` | `(ProductId, LocationId)` unique, `Version` concurrency |
| `StockMovements` | `(ProductId, LocationId, CreatedAt)` + index tambahan |
| Header transaksi | `DocNo` unique, `Status`, `Date`, `ApproverId` |
| `AuditLogs` | `(EntityName, CreatedAt)`, `UserId` + index tambahan |

## 3.5 Mapping ke Use Case

| Use Case | Objek |
|---|---|
| Master CRUD | Tabel master + index unik |
| Stock In/Out | Header/detail + `Stocks` + `StockMovements` |
| Approval | Header + `Users`/`IUserDirectory` |
| Dashboard | `Stocks`, `Products`, header Submitted |
| Laporan | `StockMovements`, `Stocks`, view |
| Audit | `AuditLogs`, `v_audit_summary` |

## 3.6 Catatan

- Nama objek harus di-quote di SQL manual (`"Products"`).
- View kompleks bersifat read-only; DML hanya pada view sederhana.
- Function/procedure dapat dipanggil dari DBeaver atau aplikasi.
