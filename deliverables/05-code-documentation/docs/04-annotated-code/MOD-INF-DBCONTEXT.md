# MOD-INF-DBCONTEXT — Audit & Soft Delete

| Field | Nilai |
|---|---|
| Module ID | `MOD-INF-DBCONTEXT` |
| File | `src/Inventory.Infrastructure/Persistence/AppDbContext.cs` |
| Method ID | `MTH-INF-DBCONTEXT-SAVE`, `MTH-INF-DBCONTEXT-AUDIT` |
| Kategori | Prosedur (side effect) |
| Eksepsi | `DbUpdateException`, `DbUpdateConcurrencyException` |

## SaveChangesAsync

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{                                                                       // awal override
    if (_auditing)                                                      // sedang menulis audit?
        return await base.SaveChangesAsync(cancellationToken);          // hindari rekursi

    var snapshots = CaptureAuditSnapshots();                            // snapshot sebelum mutasi
    ApplyAuditAndSoftDelete();                                         // isi audit + soft delete
    var result = await base.SaveChangesAsync(cancellationToken);        // simpan entity

    if (snapshots.Count > 0)                                            // ada yang perlu diaudit?
    {                                                                   // tulis audit log
        _auditing = true;                                               // tandai mode audit
        try                                                             // blok audit
        {                                                               // buka try
            foreach (var snapshot in snapshots)                         // per snapshot
            {                                                           // buka loop
                var entityId = snapshot.EntityId ?? ResolveEntityId(snapshot.Entry); // resolve id
                AuditLogs.Add(new AuditLog                              // tambah log
                {                                                       // initializer
                    EntityName = snapshot.EntityName,
                    EntityId = entityId,
                    Action = snapshot.Action,
                    OldValues = snapshot.OldValues,
                    NewValues = snapshot.NewValues,
                    UserId = _currentUser.UserId,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    CreatedAt = DateTime.UtcNow
                });                                                     // tutup log
            }                                                           // tutup loop
            await base.SaveChangesAsync(cancellationToken);             // simpan audit
        }                                                               // tutup try
        finally                                                         // selalu
        {                                                               // buka finally
            _auditing = false;                                          // reset mode
        }                                                               // tutup finally
    }                                                                   // akhir if
    return result;                                                      // kembalikan jumlah
}                                                                       // akhir method
```

## ApplyAuditAndSoftDelete

```csharp
private void ApplyAuditAndSoftDelete()
{                                                                       // awal prosedur
    var userId = _currentUser.UserId;                                   // user aktif
    var now = DateTime.UtcNow;                                          // waktu UTC

    foreach (var entry in ChangeTracker.Entries().ToList())             // iterasi perubahan
    {                                                                   // buka loop
        if (entry.Entity is IAuditableEntity auditable)                 // entity audit?
        {                                                               // isi field audit
            if (entry.State == EntityState.Added)                       // data baru?
            {                                                           // set create
                auditable.CreatedAt = now;
                auditable.CreatedById = userId;
            }                                                           // akhir if
            else if (entry.State == EntityState.Modified)               // data diubah?
            {                                                           // set update
                auditable.UpdatedAt = now;
                auditable.UpdatedById = userId;
            }                                                           // akhir else-if
        }                                                               // akhir if

        if (entry.State != EntityState.Deleted || entry.Entity is not ISoftDeletable soft)
            continue;                                                   // bukan delete/soft-deletable

        entry.State = EntityState.Modified;                             // ubah delete → update
        soft.IsDeleted = true;                                          // tandai dihapus
        soft.DeletedAt = now;                                           // waktu hapus
        soft.DeletedById = userId;                                      // user hapus

        var isActiveProperty = entry.Metadata.FindProperty("IsActive"); // cek properti IsActive
        if (isActiveProperty is not null)                               // jika ada
            entry.Property("IsActive").CurrentValue = false;            // nonaktifkan
    }                                                                   // tutup loop
}                                                                       // akhir prosedur
```

## Eksepsi

- `DbUpdateException`: constraint/index/FK gagal.
- `DbUpdateConcurrencyException`: `Stocks.Version` berubah.
- `ValidationException` tidak ditangani di sini (dilempar service).
