# MOD-APP-GUARD — Guard

| Field | Nilai |
|---|---|
| Module ID | `MOD-APP-GUARD` |
| File | `src/Inventory.Application/Common/Guard.cs` |
| Method ID | `FN-APP-GUARD-REQUIRED`, `FN-APP-GUARD-POSITIVE`, `FN-APP-GUARD-NOTNEGATIVE` |
| Kategori | Fungsi validasi |
| Eksepsi | `ValidationException` |

```csharp
public static void Required(string? value, string field)
{                                                                       // awal fungsi
    if (string.IsNullOrWhiteSpace(value))                               // kosong/whitespace?
        throw new ValidationException($"{field} wajib diisi.");         // lempar validasi
}                                                                       // akhir fungsi

public static void Positive(int value, string field)
{                                                                       // awal fungsi int
    if (value <= 0)                                                     // nol/negatif?
        throw new ValidationException($"{field} harus lebih besar dari 0.");
}                                                                       // akhir fungsi

public static void Positive(decimal value, string field)
{                                                                       // awal fungsi decimal
    if (value <= 0)                                                     // nol/negatif?
        throw new ValidationException($"{field} harus lebih besar dari 0.");
}                                                                       // akhir fungsi

public static void NotNegative(int value, string field)
{                                                                       // awal fungsi int
    if (value < 0)                                                      // negatif?
        throw new ValidationException($"{field} tidak boleh negatif.");
}                                                                       // akhir fungsi

public static void NotNegative(decimal value, string field)
{                                                                       // awal fungsi decimal
    if (value < 0)                                                      // negatif?
        throw new ValidationException($"{field} tidak boleh negatif.");
}                                                                       // akhir fungsi

public static string? TrimOrNull(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : value.Trim();             // trim/null-kan string
```

## Eksepsi

- `ValidationException` untuk field kosong, nilai ≤ 0, atau negatif.

## Kompleksitas

- O(1) waktu dan memori.
