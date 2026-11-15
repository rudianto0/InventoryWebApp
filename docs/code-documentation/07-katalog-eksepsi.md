# 07 - Katalog Eksepsi

## 1. Exception Aplikasi

| Exception | Penyebab | Modul | Penanganan |
|---|---|---|---|
| `ValidationException` | field wajib, nilai tidak valid, aturan bisnis | semua service | pesan ke user |
| `DbUpdateConcurrencyException` | `Stocks.Version` berubah | ledger | pesan konflik/retry |
| `DbUpdateException` | constraint, FK, unique index | infrastructure | log + pesan |
| `InvalidOperationException` | tipe dokumen/status tidak valid | approval/report | pesan |
| `UnauthorizedAccessException` | akses tidak sah | controller | 403 |
| `NpgsqlException` | koneksi/query DB | infrastructure | health check/log |
| Identity errors | password/role/user | `UserService` | `ValidationException` |

## 2. Eksepsi per Modul

| Module ID | Exception utama |
|---|---|
| `MOD-APP-LEDGER` | `ValidationException` |
| `MOD-APP-STOCKIN` | `ValidationException` |
| `MOD-APP-STOCKOUT` | `ValidationException` |
| `MOD-APP-TRANSFER` | `ValidationException` |
| `MOD-APP-ADJUSTMENT` | `ValidationException` |
| `MOD-APP-OPNAME` | `ValidationException` |
| `MOD-APP-APPROVAL` | `ValidationException` |
| `MOD-APP-GUARD` | `ValidationException` |
| `MOD-INF-DBCONTEXT` | `DbUpdateException`, `DbUpdateConcurrencyException` |
| `MOD-INF-USER` | `ValidationException` |
| `MOD-INF-DOCNUM` | `DbUpdateException` |
| `MOD-WEB-UI` | menangkap exception → pesan UI |

## 3. Contoh Pesan

```text
Quantity harus lebih besar dari 0.
Stok tidak mencukupi untuk produk 'A4 Paper Ream' di lokasi 'Gudang Pusat'.
Anda tidak dapat menyetujui transaksi milik sendiri.
Dokumen hanya dapat diubah saat berstatus Draft atau Rejected.
```

## 4. Strategi Penanganan

1. Validasi input di service (fail fast).
2. Gunakan transaksi untuk operasi multi-tabel.
3. Tangkap exception di controller dan tampilkan pesan.
4. Log exception tak terduga.
5. Jangan menampilkan stack trace ke user.
