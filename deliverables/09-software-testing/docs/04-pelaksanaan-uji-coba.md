# 4. Laksanakan Prosedur Uji Coba

## a. Desain skenario uji coba

Skenario dibuat berdasarkan fungsi utama aplikasi Inventory:

| Kelompok | Contoh pengujian | Hasil yang diharapkan |
|---|---|---|
| Validasi | Jumlah 0 atau negatif | Ditolak dengan pesan jelas |
| Barang masuk | Simpan dan setujui | Stok bertambah |
| Barang keluar | Stok kurang | Ditolak |
| Transfer | Pindah antar lokasi | Saldo asal turun, tujuan naik |
| Adjustment | Selisih hasil hitung | Selisih benar |
| Opname | Hitung fisik | Saldo sesuai hasil hitung |
| Persetujuan | Bukan penyetuju | Ditolak |
| Master data | Kode duplikat | Ditolak |
| Hak akses | Staff dan Admin | Sesuai peran |

## b. Desain prosedur uji coba dalam algoritma

```
PROCEDURE JalankanTest(test)
    SIAPKAN:
        buat database sementara
        isi data uji
        buat pengguna dan atasan palsu
        siapkan service yang diuji
    JALANKAN:
        panggil method yang diuji
        catat hasil atau kesalahan
    PERIKSA:
        cocokkan hasil dengan yang diharapkan
        periksa perubahan data di database
    BERSIHKAN:
        tutup dan hapus database sementara
END PROCEDURE
```

## c. Laksanakan uji coba

**Persiapan:**

```powershell
dotnet restore
dotnet build Inventory.sln
```

**Menjalankan pengujian:**

```powershell
dotnet test tests/Inventory.UnitTests/Inventory.UnitTests.csproj `
  --logger "trx;LogFileName=test-results.trx" `
  --collect:"XPlat Code Coverage"
```

**Hasil eksekusi:**

```text
Passed! - Failed: 0, Passed: 171, Skipped: 0, Total: 171
Duration: 10 s
```

**File hasil:**

| File | Isi |
|---|---|
| `results/test-results.trx` | Hasil lengkap pengujian |
| `results/test-results.csv` | Daftar 171 test dan statusnya |
| `results/test-summary.csv` | Ringkasan jumlah dan cakupan |
| `results/coverage.cobertura.xml` | Data cakupan kode |
| `results/coverage-summary.txt` | Ringkasan cakupan |

**Kondisi yang diuji:** data valid, batas, tidak valid, aktif/nonaktif,
terhapus, berbagai peran, status dokumen, stok cukup/kurang, duplikat, dan
perubahan bersamaan.
