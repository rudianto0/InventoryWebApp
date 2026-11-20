# 3. Menerapkan Teknik Dasar Algoritma Umum

## 3.1 Membuat algoritma untuk sorting

**Bubble Sort** membandingkan dua angka yang bersebelahan. Jika urutannya
salah, kedua angka ditukar. Proses diulang sampai tidak ada yang perlu ditukar.

```
PROCEDURE BubbleSort(A, n)
    FOR i ← 0 TO n-2 DO
        FOR j ← 0 TO n-2-i DO
            IF A[j] > A[j+1] THEN
                TUKAR(A[j], A[j+1])
            END IF
        END FOR
    END FOR
END PROCEDURE
```

```csharp
void BubbleSort(int[] A)
{
    for (var i = 0; i < A.Length - 1; i++)
        for (var j = 0; j < A.Length - 1 - i; j++)
            if (A[j] > A[j + 1])
                (A[j], A[j + 1]) = (A[j + 1], A[j]);
}
```

Contoh: data `5, 2, 8, 1` setelah sorting menjadi `1, 2, 5, 8`.

![Flowchart Sorting](assets/flowchart-sorting.png)

## 3.2 Membuat algoritma untuk searching

**Linear Search** memeriksa angka satu per satu dari awal sampai ditemukan.

```
FUNCTION LinearSearch(A, n, target)
    FOR i ← 0 TO n-1 DO
        IF A[i] = target THEN
            RETURN i
        END IF
    END FOR
    RETURN -1
END FUNCTION
```

```csharp
int LinearSearch(int[] A, int target)
{
    for (var i = 0; i < A.Length; i++)
        if (A[i] == target) return i;
    return -1;
}
```

**Binary Search** hanya boleh dipakai jika data sudah terurut. Data dibagi dua
berulang kali sehingga pencarian jauh lebih cepat.

```
FUNCTION BinarySearch(A, n, target)
    low ← 0
    high ← n - 1
    WHILE low <= high DO
        mid ← (low + high) / 2
        IF A[mid] = target THEN RETURN mid
        IF A[mid] < target THEN low ← mid + 1
        ELSE high ← mid - 1
    END WHILE
    RETURN -1
END FUNCTION
```

Hasil ditampilkan:

- Jika `posisi >= 0`: **"Angka ditemukan"**.
- Jika `posisi = -1`: **"Angka tidak ditemukan"**.

![Flowchart Searching](assets/flowchart-searching.png)
