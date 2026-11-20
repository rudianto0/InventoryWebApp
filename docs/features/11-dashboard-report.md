# Feature 11 - Dashboard & Report

- **ID:** F-11
- **Phase:** 4
- **Status:** Implemented
- **Depends on:** F-06, F-07, F-08, F-09, F-10

## 1. Ringkasan

Dashboard ringkas dan laporan stok untuk monitoring. Semua data read-only,
dapat difilter per lokasi/branch dan rentang tanggal.

## 2. Scope

**In scope**
- Dashboard: total produk, total stok, item di bawah reorder level, transaksi pending approval.
- Laporan: Stock Balance, Kartu Stok (Stock Card), Mutasi per periode, Reorder Alert.
- Export CSV (opsional) untuk laporan utama.

**Out of scope**
- BI engine, scheduled email report, grafik kompleks realtime.

## 3. Aktor & Hak Akses

| Aktor | Hak Akses |
|-------|-----------|
| Admin / Manager | Semua laporan |
| SPV | Laporan lokasi terkait |
| Staff | Stock balance & kartu stok |
| Auditor | Read-only semua laporan |

## 4. Data Model (read-only)

Sumber: `Stocks`, `StockMovements`, header transaksi, `Products`, `Locations`.
Data yang sudah soft-deleted otomatis tidak masuk laporan karena global query
filter `IsDeleted = false`.

### 4.1 Field Dictionary - ReportFilter

| Field | Type | Required | Constraint / Default | Fungsi |
|-------|------|----------|----------------------|--------|
| `LocationId` | Guid? | Tidak | FK Locations | Filter satu lokasi |
| `BranchCode` | string? | Tidak | - | Filter cabang |
| `ProductId` | Guid? | Tidak | FK Products | Filter satu produk |
| `From` | DateOnly? | Tidak | `<= To` | Awal rentang laporan |
| `To` | DateOnly? | Tidak | `>= From` | Akhir rentang laporan |
| `Page` | int | Ya | Default `1` | Halaman data |
| `PageSize` | int | Ya | Default `20`, maks `200` | Jumlah baris per halaman |

### 4.2 Field Dictionary - Output Laporan

| Field | Type | Fungsi |
|-------|------|--------|
| `ProductId` | Guid | ID produk pada baris laporan |
| `Sku` | string | Kode produk |
| `ProductName` | string | Nama produk |
| `UnitName` | string | Satuan produk |
| `LocationId` | Guid | ID lokasi baris laporan |
| `LocationName` | string | Nama lokasi |
| `BranchCode` | string? | Kode cabang |
| `Quantity` | int | Saldo/mutasi kuantitas |
| `UpdatedAt` | DateTime | Waktu update saldo (stock balance) |
| `CreatedAt` | DateTime | Waktu mutasi (stock card/movement) |
| `MovementNo` | string | Nomor mutasi |
| `Type` | enum | Jenis mutasi |
| `RunningBalance` | int | Saldo berjalan pada kartu stok |
| `ReorderLevel` | int | Ambang reorder produk |

### 4.3 Field Dictionary - DashboardSummary

| Field | Type | Fungsi |
|-------|------|--------|
| `TotalProducts` | int | Jumlah produk aktif |
| `TotalStockQuantity` | int | Total saldo stok |
| `BelowReorderCount` | int | Jumlah baris di bawah reorder |
| `PendingApprovals` | int | Total dokumen Submitted |
| `TotalLocations` | int | Jumlah lokasi aktif |
| `ReorderItems` | list | Daftar item reorder alert |
| `RecentMovements` | list | Mutasi stok terbaru |

## 5. Workflow

1. User membuka `/dashboard`.
2. Filter lokasi/tanggal (opsional, disimpan di query string).
3. Buka laporan → pilih parameter → tampilkan grid dengan paging server-side.
4. Export CSV (opsional) mengikuti filter aktif.

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/Dashboard` | Ringkasan KPI |
| GET | `/Reports/StockBalance` | Saldo stok per produk/lokasi |
| GET | `/Reports/StockCard` | Kartu stok per produk |
| GET | `/Reports/Movements` | Mutasi per periode |
| GET | `/Reports/Reorder` | Produk di bawah reorder level |
| GET | `/Reports/Export` | CSV sesuai filter |

### Filter umum
```csharp
public sealed class ReportFilter
{
    public Guid? LocationId { get; set; }
    public string? BranchCode { get; set; }
    public Guid? ProductId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
```

## 7. Validasi & Business Rules

- `From <= To`.
- Paging & sorting dilakukan di server.
- Stock card menampilkan saldo berjalan (running balance) per mutasi.
- Reorder menampilkan produk dengan `Stocks.Quantity <= Products.ReorderLevel`.

## 8. Edge Cases & Error Handling

- Filter kosong → default bulan berjalan.
- Data besar → batasi rentang maksimum (mis. 1 tahun) atau paksa paging.
- Lokasi non-aktif tetap muncul di laporan historis.

## 9. Security Considerations

- Semua endpoint read-only dan terproteksi login.
- Batasi laporan sensitif ke role tertentu.
- Export tidak boleh mengekspos data di luar hak akses user.

## 10. Acceptance Criteria

- [ ] Dashboard menampilkan KPI utama.
- [ ] Stock balance akurat sesuai `Stocks`.
- [ ] Stock card menampilkan running balance benar.
- [ ] Reorder alert sesuai reorder level.
- [ ] Filter lokasi/tanggal berfungsi.