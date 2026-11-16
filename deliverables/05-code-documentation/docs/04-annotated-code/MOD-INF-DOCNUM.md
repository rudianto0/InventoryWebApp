# MOD-INF-DOCNUM — DocumentNumberService.NextAsync

| Field | Nilai |
|---|---|
| Module ID | `MOD-INF-DOCNUM` |
| File | `src/Inventory.Infrastructure/Services/DocumentNumberService.cs` |
| Method ID | `MTH-INF-DOCNUM-NEXT` |
| Kategori | Fungsi/prosedur (menghasilkan nomor + update sequence) |
| Parameter | `string prefix`, `DateTime date`, `CancellationToken ct` |
| Return | `Task<string>` |
| Eksepsi | `DbUpdateException` |

```csharp
public async Task<string> NextAsync(string prefix, DateTime date, CancellationToken ct = default)
{                                                                       // awal method
    var normalized = prefix.Trim().ToUpperInvariant();                  // normalisasi prefix
    var year = date.Year;                                               // tahun
    var month = date.Month;                                             // bulan

    var sequence = await db.DocumentSequences                           // cari sequence
        .FirstOrDefaultAsync(x => x.Prefix == normalized && x.Year == year && x.Month == month, ct);

    if (sequence is null)                                               // belum ada?
    {                                                                   // buat baru
        sequence = new DocumentSequence { Prefix = normalized, Year = year, Month = month, LastNumber = 0 };
        db.DocumentSequences.Add(sequence);                             // tambah ke context
    }                                                                   // akhir if

    sequence.LastNumber++;                                              // increment
    await db.SaveChangesAsync(ct);                                      // simpan sequence
    return $"{normalized}-{year:0000}{month:00}-{sequence.LastNumber:0000}"; // format nomor
}                                                                       // akhir method
```

## Catatan

- Pola read-modify-write rentan konflik pada submit bersamaan.
- Rekomendasi: atomic increment/sequence (lihat dokumen database-access).

## Eksepsi

- `DbUpdateException` jika unique index `(Prefix, Year, Month)` dilanggar.
