# 03 - Algoritma Sorting dan Searching

## 3.1 Sorting

### Kebutuhan di aplikasi

- Daftar produk diurutkan berdasarkan `Sku`.
- Daftar transaksi diurutkan berdasarkan `Date` menurun.
- Reorder alert diurutkan berdasarkan `Quantity` naik.
- Movement diurutkan berdasarkan `CreatedAt`.

Implementasi aplikasi memakai LINQ `OrderBy`/`OrderByDescending`, yang di
belakang layar memakai algoritma pustaka .NET (introsort: quick sort +
heap sort + insertion sort).

### Pseudocode Bubble Sort

```
PROCEDURE BubbleSort(A):
    n ← LENGTH(A)
    FOR i ← 0 TO n-2 DO
        FOR j ← 0 TO n-2-i DO
            IF A[j] > A[j+1] THEN
                SWAP(A[j], A[j+1])
            END IF
        END FOR
    END FOR
END PROCEDURE
```

Kompleksitas: O(n²) waktu, O(1) memori.

### Pseudocode Merge Sort

```
PROCEDURE MergeSort(A):
    IF LENGTH(A) <= 1 THEN RETURN A
    mid ← LENGTH(A) / 2
    left ← MergeSort(A[0..mid-1])
    right ← MergeSort(A[mid..])
    RETURN Merge(left, right)
END PROCEDURE

PROCEDURE Merge(L, R):
    result ← []
    WHILE L not empty AND R not empty DO
        IF L[0] <= R[0] THEN append L[0]; remove L[0]
        ELSE append R[0]; remove R[0]
    END WHILE
    append sisa L dan R ke result
    RETURN result
END PROCEDURE
```

Kompleksitas: O(n log n) waktu, O(n) memori.

### Contoh C# manual

```csharp
public static void BubbleSort(int[] items)
{
    for (var i = 0; i < items.Length - 1; i++)
    {
        for (var j = 0; j < items.Length - 1 - i; j++)
        {
            if (items[j] > items[j + 1])
            {
                (items[j], items[j + 1]) = (items[j + 1], items[j]);
            }
        }
    }
}
```

### Contoh LINQ (dipakai aplikasi)

```csharp
var products = await db.Products
    .AsNoTracking()
    .OrderBy(p => p.Sku)
    .ToListAsync(ct);

var transactions = await db.StockInHeaders
    .AsNoTracking()
    .OrderByDescending(x => x.Date)
    .ThenByDescending(x => x.CreatedAt)
    .ToListAsync(ct);
```

### Contoh SQL

```sql
SELECT "Sku", "Name", "ReorderLevel"
FROM "Products"
WHERE "IsDeleted" = false
ORDER BY "Sku" ASC;

SELECT "DocNo", "Date", "Status"
FROM "StockInHeaders"
WHERE "IsDeleted" = false
ORDER BY "Date" DESC, "CreatedAt" DESC;
```

Index yang membantu:

```sql
CREATE INDEX IF NOT EXISTS "IX_Products_Sku_Active"
    ON "Products" ("Sku")
    WHERE "IsDeleted" = false;
```

## 3.2 Searching

### Linear Search

```
FUNCTION LinearSearch(A, target):
    FOR i ← 0 TO LENGTH(A)-1 DO
        IF A[i] = target THEN RETURN i
    END FOR
    RETURN -1
END FUNCTION
```

Kompleksitas: O(n).

### Binary Search

```
FUNCTION BinarySearch(A, target):
    low ← 0
    high ← LENGTH(A)-1
    WHILE low <= high DO
        mid ← (low + high) / 2
        IF A[mid] = target THEN RETURN mid
        IF A[mid] < target THEN low ← mid + 1
        ELSE high ← mid - 1
    END WHILE
    RETURN -1
END FUNCTION
```

Kompleksitas: O(log n), syarat data terurut.

### Contoh C#

```csharp
public static int LinearSearch(IReadOnlyList<int> items, int target)
{
    for (var i = 0; i < items.Count; i++)
        if (items[i] == target) return i;
    return -1;
}

public static int BinarySearch(int[] sorted, int target)
{
    var low = 0;
    var high = sorted.Length - 1;
    while (low <= high)
    {
        var mid = low + (high - low) / 2;
        if (sorted[mid] == target) return mid;
        if (sorted[mid] < target) low = mid + 1;
        else high = mid - 1;
    }
    return -1;
}
```

### Contoh LINQ

```csharp
var product = await db.Products.AsNoTracking()
    .FirstOrDefaultAsync(p => p.Sku == "SKU-001", ct);

var exists = await db.Products.AsNoTracking()
    .AnyAsync(p => p.Sku == "SKU-001", ct);

var matches = await db.Products.AsNoTracking()
    .Where(p => p.Name.Contains("Paper"))
    .ToListAsync(ct);
```

### Contoh SQL

```sql
-- Pencarian exact (memanfaatkan index unique)
SELECT *
FROM "Products"
WHERE "Sku" = 'SKU-001';

-- Pencarian pola (bisa memakai index jika pola prefix)
SELECT *
FROM "Products"
WHERE "Name" LIKE 'A4%';

-- Pencarian di view
SELECT *
FROM "v_stock_balance"
WHERE "Sku" = 'SKU-001';
```

### Perbandingan

| Metode | Kompleksitas | Syarat | Dipakai |
|---|---|---|---|
| Linear search | O(n) | - | Data kecil/tidak terurut |
| Binary search | O(log n) | Terurut | Array terurut |
| LINQ `FirstOrDefault` | O(n) memori / query DB | - | Kode aplikasi |
| Index B-tree DB | O(log n) | Ada index | `WHERE Sku = ...` |

## 3.3 Ringkasan

- Sorting LINQ dipakai untuk kebutuhan aplikasi; algoritma manual dipelajari
  untuk memahami biaya.
- Searching sebaiknya memanfaatkan index database untuk data besar.
- Binary search hanya berlaku pada data terurut.
- Pilihan algoritma memengaruhi waktu dan memori (lihat dokumen 05).
