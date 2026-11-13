using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Dtos;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class LocationService(IAppDbContext db)
{
    public async Task<IReadOnlyList<Location>> ListAsync(LocationType? type = null, bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = db.Locations.AsNoTracking().Include(x => x.ParentLocation).AsQueryable();
        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);
        if (activeOnly == true)
            query = query.Where(x => x.IsActive);
        return await query.OrderBy(x => x.Code).ToListAsync(ct);
    }

    public Task<Location?> GetAsync(int id, CancellationToken ct = default) =>
        db.Locations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Location> CreateAsync(LocationInput input, CancellationToken ct = default)
    {
        await ValidateAsync(input, null, ct);
        var entity = Map(new Location(), input);
        db.Locations.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(int id, LocationInput input, CancellationToken ct = default)
    {
        var entity = await db.Locations.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Lokasi tidak ditemukan.");
        await ValidateAsync(input, id, ct);
        Map(entity, input);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Locations.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Lokasi tidak ditemukan.");
        if (await db.Locations.AnyAsync(x => x.ParentLocationId == id && x.IsActive, ct))
            throw new ValidationException("Lokasi masih memiliki sub-lokasi aktif. Non-aktifkan sub-lokasi terlebih dahulu.");
        if (await db.Stocks.AnyAsync(x => x.LocationId == id && x.Quantity != 0, ct))
            throw new ValidationException("Lokasi masih memiliki saldo stok. Pindahkan atau sesuaikan stok terlebih dahulu.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SelectOption>> GetOptionsAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var query = db.Locations.AsNoTracking().AsQueryable();
        if (activeOnly)
            query = query.Where(x => x.IsActive);
        return await query.OrderBy(x => x.Code)
            .Select(x => new SelectOption { Id = x.Id, Text = x.Code + " - " + x.Name, IsActive = x.IsActive })
            .ToListAsync(ct);
    }

    private async Task ValidateAsync(LocationInput input, int? id, CancellationToken ct)
    {
        Guard.Required(input.Code, "Kode lokasi");
        Guard.Required(input.Name, "Nama lokasi");
        var code = input.Code.Trim().ToUpperInvariant();
        if (await db.Locations.AnyAsync(x => x.Code == code && (!id.HasValue || x.Id != id.Value), ct))
            throw new ValidationException("Kode lokasi sudah digunakan.");

        if (input.ParentLocationId.HasValue)
        {
            if (input.ParentLocationId.Value == id)
                throw new ValidationException("Lokasi tidak boleh menjadi parent dirinya sendiri.");

            var parent = await db.Locations.FirstOrDefaultAsync(x => x.Id == input.ParentLocationId.Value, ct)
                ?? throw new ValidationException("Parent lokasi tidak ditemukan.");
            if (!parent.IsActive)
                throw new ValidationException("Parent lokasi tidak aktif.");
            if (input.Type != LocationType.Branch && parent.Type != LocationType.Branch)
                throw new ValidationException("Parent lokasi harus bertipe Branch.");

            if (id.HasValue && await CreatesCycleAsync(id.Value, parent.Id, ct))
                throw new ValidationException("Hierarki lokasi membentuk siklus.");
        }
    }

    private async Task<bool> CreatesCycleAsync(int currentId, int parentId, CancellationToken ct)
    {
        var visited = new HashSet<int>();
        var cursor = (int?)parentId;
        while (cursor.HasValue)
        {
            if (cursor.Value == currentId)
                return true;
            if (!visited.Add(cursor.Value))
                return true;
            cursor = await db.Locations.Where(x => x.Id == cursor.Value).Select(x => x.ParentLocationId).FirstOrDefaultAsync(ct);
        }
        return false;
    }

    private static Location Map(Location entity, LocationInput input)
    {
        entity.Code = input.Code.Trim().ToUpperInvariant();
        entity.Name = input.Name.Trim();
        entity.Type = input.Type;
        entity.ParentLocationId = input.ParentLocationId;
        entity.BranchCode = Guard.TrimOrNull(input.BranchCode)?.ToUpperInvariant();
        entity.Address = Guard.TrimOrNull(input.Address);
        entity.IsActive = input.IsActive;
        return entity;
    }
}
