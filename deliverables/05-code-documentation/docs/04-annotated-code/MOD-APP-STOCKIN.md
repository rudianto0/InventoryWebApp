# MOD-APP-STOCKIN — Submit & Approve

| Field | Nilai |
|---|---|
| Module ID | `MOD-APP-STOCKIN` |
| File | `src/Inventory.Application/Services/StockInService.cs` |
| Method ID | `MTH-APP-STOCKIN-SUBMIT`, `MTH-APP-STOCKIN-APPROVE` |
| Kategori | Prosedur |
| Parameter | `Guid id`, `string? notes`, `CancellationToken ct` |
| Eksepsi | `ValidationException` |

## SubmitAsync

```csharp
public async Task SubmitAsync(Guid id, CancellationToken ct = default)
{                                                                       // awal prosedur
    var header = await db.StockInHeaders.Include(x => x.Details)         // load header + detail
        .FirstOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new ValidationException("Dokumen Stock In tidak ditemukan."); // gagal jika tidak ada

    ApprovalGuard.EnsureOwner(header.CreatedById, CurrentUserId, IsAdmin, "submit"); // cek pemilik
    ApprovalGuard.EnsureEditable(header);                               // hanya Draft/Rejected
    await ValidateStoredAsync(header, ct);                              // validasi data tersimpan

    if (IsAdmin)                                                        // admin tanpa approval?
    {                                                                   // langsung approve
        await ApplyApprovalAsync(header, null, ct);                     // terapkan stok
        return;                                                         // selesai
    }                                                                   // akhir if

    await ApprovalGuard.SubmitAsync(header, CurrentUserId, users, ct);  // cari approver + Submitted
    await db.SaveChangesAsync(ct);                                      // simpan status
}                                                                       // akhir prosedur
```

## ApplyApprovalAsync

```csharp
private async Task ApplyApprovalAsync(StockInHeader header, string? notes, CancellationToken ct)
{                                                                       // awal prosedur
    await using var transaction = await db.BeginTransactionAsync(ct);   // mulai transaksi
    try                                                                 // blok sukses
    {                                                                   // buka try
        ApprovalGuard.MarkApproved(header, CurrentUserId);              // set status/approved
        if (!string.IsNullOrWhiteSpace(notes))                          // ada catatan?
            header.Notes = notes.Trim();                                // simpan catatan
        await ledger.ApplyAsync(header.Details.Select(d => new StockChange( // mutasi stok
            MovementType.In, ReferenceType.StockIn, header.Id,          // tipe + referensi
            d.ProductId, header.LocationId, d.Quantity,                 // produk, lokasi, qty
            CurrentUserId, d.Notes)), ct);                              // user + catatan
        await db.SaveChangesAsync(ct);                                  // simpan semua
        await transaction.CommitAsync(ct);                              // commit
    }                                                                   // tutup try
    catch                                                               // jika gagal
    {                                                                   // buka catch
        await transaction.RollbackAsync(ct);                            // batalkan transaksi
        throw;                                                          // teruskan error
    }                                                                   // tutup catch
}                                                                       // akhir prosedur
```

## Eksepsi

- `ValidationException`: dokumen tidak ditemukan, bukan owner, status tidak editable,
  supplier/lokasi/produk tidak valid, quantity ≤ 0, approver tidak valid.
- `DbUpdateConcurrencyException`: konflik stok.

## Kompleksitas

- O(n) validasi + O(k) ledger.
- Transaksi memastikan atomik.
