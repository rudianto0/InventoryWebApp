# 1. Lakukan Identifikasi Kode Program

Contoh pada skenario menyebut aplikasi pembayaran listrik. Standar dokumentasi
yang sama diterapkan pada aplikasi **Inventory** yang dikembangkan.

## a. Identifikasi modul yang ada pada program

Program dibagi menjadi beberapa modul agar kode mudah dicari dan dirawat.
Setiap modul memiliki ID unik.

| Module ID | Modul | File utama | Kegunaan |
|---|---|---|---|
| `MOD-DOM-INVENTORY` | Stok dan mutasi | `Domain/Entities/Inventory.cs` | Menyimpan data stok dan pergerakan |
| `MOD-APP-CATALOG` | Katalog | `Application/Services/CatalogService.cs` | Mengelola produk, kategori, satuan |
| `MOD-APP-LEDGER` | Mutasi stok | `Application/Services/StockLedger.cs` | Mengubah saldo dan mencatat pergerakan |
| `MOD-APP-STOCKIN` | Barang masuk | `Application/Services/StockInService.cs` | Mencatat penerimaan barang |
| `MOD-APP-STOCKOUT` | Barang keluar | `Application/Services/StockOutService.cs` | Mencatat pengeluaran barang |
| `MOD-APP-TRANSFER` | Transfer | `Application/Services/StockTransferService.cs` | Memindahkan stok antar lokasi |
| `MOD-APP-APPROVAL` | Persetujuan | `ApprovalService.cs`, `ApprovalGuard` | Mengatur persetujuan satu tingkat |
| `MOD-APP-REPORT` | Laporan | `Application/Services/ReportService.cs` | Menampilkan saldo dan kartu stok |
| `MOD-APP-GUARD` | Validasi | `Application/Common/Guard.cs` | Memeriksa data wajib dan nilai |
| `MOD-INF-DBCONTEXT` | Audit dan soft delete | `Infrastructure/Persistence/AppDbContext.cs` | Menyimpan riwayat dan menandai hapus |
| `MOD-INF-DOCNUM` | Nomor dokumen | `Infrastructure/Services/DocumentNumberService.cs` | Membuat nomor dokumen otomatis |
| `MOD-WEB-UI` | Tampilan web | `Inventory.Web/*` | Controller dan halaman pengguna |

Total ada 24 modul logis. Tabel di atas menampilkan modul yang paling sering
digunakan.

## b. Identifikasi parameter yang digunakan pada program

| Data masukan | Isi utama | Keterangan sederhana |
|---|---|---|
| `StockInInput` | supplier, lokasi, tanggal, daftar barang | Data barang masuk |
| `StockOutInput` | lokasi, tujuan, daftar barang | Data barang keluar |
| `StockTransferInput` | lokasi asal, lokasi tujuan, daftar barang | Data perpindahan barang |
| `DocumentLineInput` | produk, jumlah, harga | Satu baris barang |
| `ReportFilter` | lokasi, produk, tanggal, halaman | Filter laporan |
| `CancellationToken` | penanda berhenti | Membatalkan proses yang lama |

Aturan penting: jumlah barang harus lebih dari 0, lokasi harus aktif, dan
stok harus cukup sebelum barang keluar.

## c. Jelaskan cara kerja algoritma program sesuai definisi masalah

**Masalah utama:** saldo stok harus selalu benar dan tidak boleh minus.

**Cara kerja mutasi stok (Stock Ledger):**

1. Ambil data saldo berdasarkan produk dan lokasi.
2. Jika belum ada, buat saldo baru dengan nilai 0.
3. Hitung saldo baru dari saldo lama ditambah perubahan.
4. Jika saldo baru negatif, hentikan proses dengan pesan kesalahan.
5. Simpan saldo baru dan catat pergerakan stok.

```
PROCEDURE Apply(changes)
    FOR EACH change IN changes DO
        stock ← FIND Stock(change.ProductId, change.LocationId)
        IF stock = NULL THEN stock ← NEW Stock(quantity=0)
        newQuantity ← stock.Quantity + change.Quantity
        IF newQuantity < 0 THEN THROW "Stok tidak mencukupi"
        stock.Quantity ← newQuantity
        INSERT StockMovement(change)
    END FOR
END PROCEDURE
```

**Cara kerja barang masuk:** data divalidasi, disimpan sebagai draft, diajukan
untuk persetujuan, lalu saldo ditambah setelah disetujui.

**Cara kerja barang keluar:** stok diperiksa lebih dulu. Jika stok tidak cukup,
proses ditolak. Jika cukup, saldo dikurangi setelah disetujui.

## d. Berikan komentar setiap baris kode

Komentar baris demi baris membantu pengembang lain memahami kode. Contoh untuk
prosedur mutasi stok:

```csharp
public async Task ApplyAsync(IEnumerable<StockChange> changes, CancellationToken ct = default)
{                                                       // awal prosedur
    foreach (var change in changes)                     // ulangi setiap perubahan
    {
        if (change.Quantity == 0)                       // jumlah nol?
            continue;                                   // lewati, tidak ada mutasi

        var stock = await db.Stocks.FirstOrDefaultAsync( // cari saldo yang ada
            x => x.ProductId == change.ProductId &&
                 x.LocationId == change.LocationId, ct);

        if (stock is null)                              // saldo belum ada?
        {
            stock = new Stock { Quantity = 0 };         // buat saldo baru
            db.Stocks.Add(stock);                       // tambahkan ke database
        }

        var newQuantity = stock.Quantity + change.Quantity; // hitung saldo baru
        if (newQuantity < 0)                            // cek agar tidak minus
            throw new ValidationException("Stok tidak mencukupi."); // eksepsi

        stock.Quantity = newQuantity;                   // simpan saldo baru
        db.StockMovements.Add(new StockMovement { ... }); // catat pergerakan
    }
}
```

Komentar lengkap untuk 7 modul inti tersedia di folder
`docs/04-annotated-code/`. Setiap entri menjelaskan **data**, **class**,
**fungsi/prosedur**, dan **eksepsi** yang mungkin muncul.
