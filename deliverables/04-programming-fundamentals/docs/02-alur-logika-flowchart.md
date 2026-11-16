# 02 - Alur Logika (Flowchart)

## 2.1 Metode yang Dipilih

Metode: **flowchart standar** dengan simbol:

| Simbol | Komponen |
|---|---|
| Oval | Start / End |
| Persegi panjang | Process |
| Jajar genjang | Input / Output |
| Belah ketupat | Decision |
| Panah | Alur |
| Lingkaran kecil | Connector |

Flowchart dibuat untuk alur yang paling penting di aplikasi:

1. Stock In → Submit → Approval → Update stok.
2. Stock Out → validasi stok → Approval → Update stok.
3. Sorting/Searching data produk.

## 2.2 Komponen yang Dibutuhkan

| Komponen | Contoh di aplikasi |
|---|---|
| Input | user, supplier, lokasi, produk, quantity |
| Process | validasi, simpan draft, submit, approve, ledger |
| Decision | stok cukup? approver valid? parent ada? |
| Output | dokumen berhasil, pesan validasi, status |
| Data store | `StockInHeaders`, `Stocks`, `StockMovements` |

## 2.3 Relasi Antar Komponen

- Input menghasilkan Draft.
- Submit memerlukan atasan (parent user).
- Approve memerlukan approver yang sesuai.
- Approve memicu `StockLedger` yang mengubah `Stocks` dan `StockMovements`.
- Reject mengubah status tanpa mutasi stok.

## 2.4 Alur Mulai dan Selesai

- Setiap alur dimulai dari **Start** (user membuka form).
- Setiap alur berakhir di **End** (sukses/gagal).
- Alur gagal tetap memiliki end (pesan error).

---

## Flowchart 1 - Stock In Approval

![Flowchart Stock In](assets/flowchart-stock-in.png)

```mermaid
flowchart TD
    A([Start]) --> B[Input supplier, lokasi, produk, qty]
    B --> C{Valid?}
    C -- Tidak --> E[Tampilkan error] --> Z([End gagal])
    C -- Ya --> D[Simpan Draft + detail]
    D --> F[Submit]
    F --> G{Atasan/SPV ada & aktif?}
    G -- Tidak --> E
    G -- Ya --> H[Status Submitted + Approver]
    H --> I{Approver setuju?}
    I -- Tidak --> J[Status Rejected] --> Z
    I -- Ya --> K[Status Approved]
    K --> L[Update Stocks]
    L --> M[Insert StockMovements]
    M --> N([End sukses])
```

### Langkah

1. User mengisi form.
2. Service memvalidasi supplier, lokasi, produk, quantity.
3. Dokumen disimpan sebagai Draft.
4. Submit mencari atasan; jika tidak ada/non-aktif → gagal.
5. Approver menyetujui/menolak.
6. Jika approve, ledger mengubah saldo dan mencatat movement.

---

## Flowchart 2 - Stock Out

![Flowchart Stock Out](assets/flowchart-stock-out.png)

```mermaid
flowchart TD
    A([Start]) --> B[Input lokasi, produk, qty]
    B --> C{Valid?}
    C -- Tidak --> E[Tampilkan error] --> Z([End gagal])
    C -- Ya --> D[Simpan Draft]
    D --> F[Submit]
    F --> G{Stok cukup?}
    G -- Tidak --> E
    G -- Ya --> H[Status Submitted]
    H --> I{Approver setuju?}
    I -- Tidak --> J[Status Rejected] --> Z
    I -- Ya --> K[Kurangi Stocks]
    K --> L[Insert StockMovements negatif]
    L --> N([End sukses])
```

### Catatan

- Pengecekan stok dilakukan saat submit **dan** approve.
- Jika saldo berubah di antara submit dan approve, approve dapat gagal.

---

## Flowchart 3 - Sorting dan Searching

![Flowchart Sorting Searching](assets/flowchart-sorting-searching.png)

```mermaid
flowchart TD
    A([Start]) --> B[Input data / kata kunci]
    B --> C{Pilih operasi}
    C -- Sorting --> D[Pilih algoritma: LINQ / Merge / Quick]
    D --> E[Urutkan data]
    C -- Searching --> F[Pilih algoritma: Linear / Binary / LINQ / Index]
    F --> G[Cari data]
    E --> H[Tampilkan hasil]
    G --> H
    H --> Z([End])
```

### Catatan

- Sorting: `OrderBy` LINQ atau algoritma manual.
- Searching: linear/binary di memori, atau query index di database.
- Binary search hanya untuk data terurut.

---

## Ringkasan Komponen

| Alur | Decision utama | Efek samping |
|---|---|---|
| Stock In | validasi, atasan, approve | stok bertambah |
| Stock Out | validasi, stok cukup, approve | stok berkurang |
| Sorting/Searching | pilih algoritma, data terurut? | tidak ada (read-only) |
