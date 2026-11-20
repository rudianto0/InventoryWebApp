# 2. Membuat Alur Logika (Flowchart)

## 2.1 Metode yang sesuai ditentukan

Metode yang dipakai adalah **flowchart standar** dengan simbol:

| Simbol | Arti |
|---|---|
| Oval | Mulai / selesai |
| Persegi panjang | Proses |
| Jajar genjang | Input / output |
| Belah ketupat | Keputusan |
| Panah | Arah alur |

## 2.2 Komponen yang dibutuhkan ditentukan

| Komponen | Contoh |
|---|---|
| Input | n, angka, pilihan menu, angka dicari |
| Proses | simpan angka, sorting, searching |
| Keputusan | pilihan menu, data ditemukan atau tidak |
| Output | hasil sorting, "Angka ditemukan/tidak" |
| Penyimpanan | array atau list angka |

## 2.3 Relasi antar komponen ditetapkan

- Menu 1: input angka lalu kembali ke menu.
- Menu 2: sorting lalu tampilkan hasil lalu kembali ke menu.
- Menu 3: input angka dicari, searching, tampilkan hasil, kembali ke menu.
- Menu 4: keluar dari program.

## 2.4 Alur mulai dan selesai ditetapkan

Program dimulai dari **Start**, menampilkan menu, lalu berulang sampai
pengguna memilih **Keluar** dan berakhir di **End**.

![Flowchart Menu Utama](assets/flowchart-menu.png)

### Pseudocode menu utama

```
PROCEDURE MenuUtama
    ulang ← true
    WHILE ulang DO
        TAMPILKAN "1. Input angka"
        TAMPILKAN "2. Sorting"
        TAMPILKAN "3. Searching"
        TAMPILKAN "4. Keluar"
        BACA pilihan

        CASE pilihan OF
            1: InputAngka()
            2: SortAngka(angka)
               TampilkanAngka(angka)
            3: BACA target
               posisi ← LinearSearch(angka, target)
               TampilkanHasil(posisi)
            4: ulang ← false
            LAINNYA: TAMPILKAN "Pilihan tidak valid"
        END CASE
    END WHILE
END PROCEDURE
```
