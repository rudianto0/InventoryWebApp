using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Services;

public sealed class DocumentNumberService(IAppDbContext db) : IDocumentNumberService
{
    public async Task<string> NextAsync(string prefix, DateTime date, CancellationToken ct = default)
    {
        var normalized = prefix.Trim().ToUpperInvariant();
        var year = date.Year;
        var month = date.Month;

        var sequence = await db.DocumentSequences
            .FirstOrDefaultAsync(x => x.Prefix == normalized && x.Year == year && x.Month == month, ct);
        if (sequence is null)
        {
            sequence = new DocumentSequence { Prefix = normalized, Year = year, Month = month, LastNumber = 0 };
            db.DocumentSequences.Add(sequence);
        }

        sequence.LastNumber++;
        await db.SaveChangesAsync(ct);
        return $"{normalized}-{year:0000}{month:00}-{sequence.LastNumber:0000}";
    }
}
