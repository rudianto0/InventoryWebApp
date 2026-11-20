# 4. Menggunakan Prosedur dan Fungsi

## 4.1 Identifikasi konsep penggunaan kembali prosedur dan fungsi

Logika yang sama tidak perlu ditulis berulang. Cukup dibuat sekali, lalu
dipanggil berkali-kali. Prinsip ini disebut **DRY (Don't Repeat Yourself)**.

| Bagian | Dipakai untuk |
|---|---|
| `InputAngka` | Menu input dan pencarian |
| `SortAngka` | Menu sorting |
| `TampilkanAngka` | Menampilkan data dan hasil sorting |
| `LinearSearch` | Mencari angka |

## 4.2 Gunakan prosedur

Prosedur adalah rutin yang melakukan tindakan. Prosedur tidak wajib
mengembalikan nilai.

```
PROCEDURE InputAngka(A, n)
    BACA n
    FOR i ← 0 TO n-1 DO
        BACA A[i]
    END FOR
END PROCEDURE

PROCEDURE SortAngka(A, n)
    CALL BubbleSort(A, n)
END PROCEDURE

PROCEDURE TampilkanHasil(posisi)
    IF posisi >= 0 THEN
        TAMPILKAN "Angka ditemukan"
    ELSE
        TAMPILKAN "Angka tidak ditemukan"
    END IF
END PROCEDURE
```

```csharp
void InputAngka(int[] A)
{
    for (var i = 0; i < A.Length; i++)
        A[i] = int.Parse(Console.ReadLine());
}

void TampilkanHasil(int posisi)
{
    Console.WriteLine(posisi >= 0
        ? "Angka ditemukan"
        : "Angka tidak ditemukan");
}
```

## 4.3 Gunakan fungsi

Fungsi adalah rutin yang **mengembalikan nilai**, misalnya posisi angka yang
dicari.

```
FUNCTION CariAngka(A, n, target)
    RETURN LinearSearch(A, n, target)
END FUNCTION
```

```csharp
int CariAngka(int[] A, int target) => LinearSearch(A, target);
```

| Aspek | Prosedur | Fungsi |
|---|---|---|
| Mengembalikan nilai | Tidak wajib | Wajib |
| Contoh | `InputAngka`, `TampilkanHasil` | `LinearSearch`, `BinarySearch` |
| Tujuan | Melakukan aksi | Menghitung atau mencari |

Dengan prosedur dan fungsi, program lebih rapi, mudah diuji, dan mudah diubah.
