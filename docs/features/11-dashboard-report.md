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

## 5. Workflow

1. User membuka `/dashboard`.
2. Filter lokasi/tanggal (opsional, disimpan di query string).
3. Buka laporan → pilih parameter → tampilkan grid dengan paging server-side.
4. Export CSV (opsional) mengikuti filter aktif.

## 6. Routing / Contract

| Method | Path | Deskripsi |
|--------|------|-----------|
| GET | `/dashboard` | Ringkasan KPI |
| GET | `/reports/stock-balance` | Saldo stok per produk/lokasi |
| GET | `/reports/stock-card` | Kartu stok per produk |
| GET | `/reports/movements` | Mutasi per periode |
| GET | `/reports/reorder` | Produk di bawah reorder level |
| GET | `/reports/export` | CSV sesuai filter |

### Filter umum
```csharp
public sealed class ReportFilter
{
    public int? LocationId { get; set; }
    public string? BranchCode { get; set; }
    public int? ProductId { get; set; }
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
