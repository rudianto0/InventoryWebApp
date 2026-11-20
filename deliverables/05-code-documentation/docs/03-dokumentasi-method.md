# 3. Buatlah Dokumentasi Fungsi, Prosedur atau Method Program

## a. Buat dokumentasi method yang memudahkan pihak lain membaca

Setiap method penting memiliki keterangan yang jelas.

| Field | Keterangan |
|---|---|
| Method ID | ID unik method |
| Signature | Nama dan parameter |
| Kategori | Fungsi atau prosedur |
| Modul | Module ID pemilik |
| Parameter | Tipe dan arti |
| Return | Nilai yang dikembalikan |
| Efek samping | Perubahan data yang terjadi |
| Eksepsi | Kesalahan yang mungkin muncul |
| Contoh | Cara memanggil |

**Method penting**

| Method ID | Method | Kategori | Kegunaan |
|---|---|---|---|
| `MTH-APP-LEDGER-APPLY` | `StockLedger.ApplyAsync` | Prosedur | Mengubah saldo dan mencatat pergerakan |
| `MTH-APP-STOCKIN-CREATE` | `StockInService.CreateAsync` | Prosedur | Menyimpan barang masuk |
| `MTH-APP-STOCKIN-SUBMIT` | `StockInService.SubmitAsync` | Prosedur | Mengajukan persetujuan |
| `MTH-APP-STOCKIN-APPROVE` | `StockInService.ApproveAsync` | Prosedur | Menyetujui dan menambah stok |
| `MTH-APP-STOCKOUT-VALIDATE` | `StockOutService.ValidateLinesAsync` | Fungsi | Memeriksa kecukupan stok |
| `MTH-APP-APPROVAL-SUBMIT` | `ApprovalGuard.SubmitAsync` | Prosedur | Menentukan atasan penyetuju |
| `MTH-APP-GUARD-POSITIVE` | `Guard.Positive` | Fungsi | Memastikan nilai lebih dari 0 |
| `MTH-INF-DOCNUM-NEXT` | `DocumentNumberService.NextAsync` | Fungsi | Membuat nomor dokumen |
| `MTH-APP-REPORT-BALANCE` | `ReportService.GetStockBalanceAsync` | Fungsi | Menampilkan saldo stok |

**Contoh entri**

- Method ID: `MTH-APP-LEDGER-APPLY`
- Signature: `ApplyAsync(IEnumerable<StockChange> changes, CancellationToken ct)`
- Kategori: Prosedur (mengubah data).
- Return: `Task` (tidak mengembalikan nilai).
- Efek samping: mengubah `Stocks` dan menambah `StockMovements`.
- Eksepsi: `ValidationException` jika saldo menjadi negatif.
- Contoh panggilan: `await ledger.ApplyAsync(changes, ct);`

## b. Menjelaskan kemungkinan eksepsi pada modul program

| Eksepsi | Penyebab | Penanganan |
|---|---|---|
| `ValidationException` | Data wajib kosong atau nilai tidak valid | Tampilkan pesan ke pengguna |
| `DbUpdateConcurrencyException` | Data stok diubah pengguna lain | Minta pengguna mencoba lagi |
| `DbUpdateException` | Aturan database dilanggar | Catat log dan tampilkan pesan |
| `UnauthorizedAccessException` | Pengguna tidak berhak | Tampilkan akses ditolak |
| `NpgsqlException` | Koneksi database bermasalah | Catat log dan periksa server |
| Identity errors | Password atau peran tidak valid | Ubah menjadi pesan validasi |

**Contoh pesan:**

- "Quantity harus lebih besar dari 0."
- "Stok tidak mencukupi untuk produk ini."
- "Anda tidak dapat menyetujui transaksi milik sendiri."
- "Dokumen hanya dapat diubah saat berstatus Draft."

## c. Revisi dokumen sesuai perubahan kode program

| Perubahan | Tindakan |
|---|---|
| Nama atau parameter method berubah | Perbarui signature dan contoh |
| Nilai kembalian berubah | Perbarui bagian return |
| Eksepsi baru | Tambahkan ke katalog eksepsi |
| Efek samping berubah | Perbarui bagian efek samping |
| Setiap revisi | Tambahkan tanggal dan versi |

Dokumen dianggap lengkap jika method penting memiliki ID, parameter, return,
efek samping, eksepsi, dan contoh pemanggilan.
