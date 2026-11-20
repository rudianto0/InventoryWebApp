# 1. Menjelaskan Varian dan Invarian

> Catatan: dokumen bagian 04 adalah **latihan algoritma terpisah** (program menu
> angka). Bubble Sort/Linear Search di bagian 03 bukan bagian dari source code
> aplikasi Inventory. Implementasi aplikasi memakai LINQ/EF Core dan algoritma
> bisnis inventori.

## 1.1 Tipe data yang sesuai kaidah pemrograman

Program menu angka memakai tipe data sederhana agar mudah diurutkan dan dicari.

| Data | Tipe | Alasan |
|---|---|---|
| Daftar angka | `int[]` atau `List<int>` | Bilangan bulat |
| Banyak angka (n) | `int` | Jumlah data |
| Pilihan menu | `int` | Angka 1 sampai 4 |
| Angka dicari | `int` | Target pencarian |
| Posisi hasil | `int` | -1 jika tidak ditemukan |
| Pesan | `string` | Teks menu dan hasil |

```csharp
int n = 5;
int[] angka = new int[n];
```

## 1.2 Variabel data yang sesuai kaidah pemrograman

| Variabel | Tipe | Fungsi |
|---|---|---|
| `angka` | `int[]` | Menyimpan semua angka |
| `n` | `int` | Banyak angka |
| `pilihan` | `int` | Menu yang dipilih |
| `target` | `int` | Angka yang dicari |
| `posisi` | `int` | Letak angka (-1 = tidak ada) |
| `tukar` | `int` | Bantuan menukar nilai |

Aturan: nama jelas, huruf kecil di awal (camelCase), dan dideklarasikan dekat
dengan pemakaian.

## 1.3 Konstanta yang sesuai kaidah pemrograman

| Konstanta | Nilai | Arti |
|---|---|---|
| `MENU_INPUT` | 1 | Input angka |
| `MENU_SORT` | 2 | Sorting |
| `MENU_SEARCH` | 3 | Searching |
| `MENU_EXIT` | 4 | Keluar |

```csharp
const int MENU_INPUT = 1;
const int MENU_SORT = 2;
const int MENU_SEARCH = 3;
const int MENU_EXIT = 4;
```

**Invarian program**

| Invarian | Arti |
|---|---|
| `n >= 0` | Jumlah data tidak boleh negatif |
| Data tidak kosong saat sorting/searching | Program memberi pesan jika kosong |
| `posisi = -1` | Angka tidak ditemukan |
| Data terurut untuk binary search | Syarat binary search |
