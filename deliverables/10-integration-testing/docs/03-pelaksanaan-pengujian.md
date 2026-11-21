# 3. Melaksanakan Pengujian

## 3.1 Jalankan modul program sesuai dengan prosedur yang ditetapkan

```powershell
dotnet clean Inventory.sln
dotnet build Inventory.sln
dotnet test tests/Inventory.UnitTests/Inventory.UnitTests.csproj `
  --results-directory deliverables/10-integration-testing/results `
  --logger "trx;LogFileName=test-results.trx" `
  --collect:"XPlat Code Coverage"
```

**Modul yang dijalankan:**

| Modul | Test |
|---|---|
| Mutasi stok | `GuardAndLedgerTests`, `CoreTests` |
| Barang masuk | `StockInServiceTests` |
| Barang keluar | `StockOutServiceTests` |
| Transfer | `StockTransferServiceTests` |
| Adjustment dan opname | `StockAdjustmentServiceTests` |
| Katalog | `CatalogServiceTests` |
| Supplier dan lokasi | `SupplierAndLocationTests` |
| Persetujuan, laporan, audit | `ApprovalServiceTests`, `ReportServiceTests` |
| Pengguna dan hak akses | `UserServiceTests` |

**Hasil eksekusi:**

```text
Passed! - Failed: 0, Passed: 171, Skipped: 0, Total: 171
Duration: 7 s
```

## 3.2 Implementasikan data atau kondisi sebagai masukan ke program

| Kondisi | Cara menerapkan |
|---|---|
| Data valid | Isi data dasar standar |
| Stok kurang | Isi saldo lebih kecil dari permintaan |
| Bukan penyetuju | Gunakan pengguna lain sebagai aktor |
| Admin | Gunakan peran Admin |
| Data terhapus | Tandai `IsDeleted = true` |
| Produk duplikat | Masukkan produk yang sama dua kali |
| Stok cukup | Isi saldo sesuai kebutuhan |

## 3.3 Catat hasil pengujian dalam lembar pengujian

| Lembar | Isi |
|---|---|
| `sheets/test-case-sheet.csv` | Skenario dan hasil yang diharapkan |
| `sheets/result-sheet.csv` | Hasil nyata dan status |
| `sheets/result-sheet-third-party.csv` | Hasil nyata uji integrasi third party |
| `sheets/defect-sheet.csv` | Ketidaksesuaian yang ditemukan |
| `sheets/equipment-sheet.csv` | Peralatan yang dipakai |
| `results/test-results.trx` | Bukti lengkap pengujian |
| `results/test-summary.csv` | Ringkasan jumlah dan cakupan |

![Alur Integrasi](assets/integration-flow.png)
