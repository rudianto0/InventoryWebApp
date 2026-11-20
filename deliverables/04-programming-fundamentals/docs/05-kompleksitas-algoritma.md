# 5. Mengidentifikasi Kompleksitas Algoritma

Kompleksitas menunjukkan seberapa cepat waktu dan seberapa besar memori yang
dibutuhkan saat jumlah data bertambah. Notasi `O(...)` dibaca "orde".

## 5.1 Identifikasi kompleksitas waktu algoritma

| Operasi | Kompleksitas waktu | Arti sederhana |
|---|---|---|
| Input n angka | O(n) | Mengulang sebanyak data |
| Bubble Sort | O(n²) | Dua perulangan bertingkat |
| Merge/Quick Sort | O(n log n) | Lebih cepat untuk data besar |
| Linear Search | O(n) | Memeriksa satu per satu |
| Binary Search | O(log n) | Membagi dua data berulang |
| Tampilkan hasil | O(n) | Menulis semua angka |

**Penjelasan:**

- Jika data 100 angka, Bubble Sort bisa memerlukan sekitar 10.000 langkah,
  sedangkan Merge Sort sekitar 700 langkah.
- Binary Search sangat cepat, tetapi data harus sudah terurut.
- Untuk data kecil, perbedaannya tidak terasa. Untuk data besar, pilihan
  algoritma sangat berpengaruh.

## 5.2 Identifikasi kompleksitas penggunaan memori algoritma

| Algoritma | Kompleksitas memori | Arti sederhana |
|---|---|---|
| Menyimpan n angka | O(n) | Satu tempat untuk setiap angka |
| Bubble Sort | O(1) | Hanya butuh variabel bantu |
| Linear Search | O(1) | Tidak perlu salinan data |
| Binary Search | O(1) | Hanya butuh batas kiri/kanan |
| Merge Sort | O(n) | Perlu array bantu |

**Kesimpulan:**

- Bubble Sort hemat memori tetapi lambat untuk data besar.
- Merge Sort lebih cepat tetapi memakai memori tambahan.
- Linear Search sederhana; Binary Search lebih cepat jika data terurut.
- Pilihan terbaik bergantung pada ukuran data dan apakah data sudah terurut.
