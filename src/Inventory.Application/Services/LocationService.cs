using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class LocationService(IAppDbContext db)
{
    public async Task<IReadOnlyList<LocationResponse>> ListAsync(LocationType? type = null, bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = db.Locations.AsNoTracking().AsQueryable();
        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);
        if (activeOnly == true)
            query = query.Where(x => x.IsActive);
        return await query.OrderBy(x => x.Code).Select(x => new LocationResponse
        {
            Id = x.Id, Code = x.Code, Name = x.Name, Type = x.Type,
            ParentLocationId = x.ParentLocationId, ParentLocationName = x.ParentLocation!.Name,
            BranchCode = x.BranchCode, Address = x.Address, IsActive = x.IsActive
        }).ToListAsync(ct);
    }

    public Task<LocationResponse?> GetAsync(Guid id, CancellationToken ct = default) =>
        db.Locations.Where(x => x.Id == id).Select(x => new LocationResponse
        {
            Id = x.Id, Code = x.Code, Name = x.Name, Type = x.Type,
            ParentLocationId = x.ParentLocationId, ParentLocationName = x.ParentLocation!.Name,
            BranchCode = x.BranchCode, Address = x.Address, IsActive = x.IsActive
        }).FirstOrDefaultAsync(ct);

    public async Task<LocationResponse> CreateAsync(LocationInput input, CancellationToken ct = default)
    {
        await ValidateAsync(input, null, ct);
        var entity = Map(new Location(), input);
        db.Locations.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapResponse(entity, null);
    }

    public async Task UpdateAsync(Guid id, LocationInput input, CancellationToken ct = default)
    {
        var entity = await db.Locations.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Lokasi tidak ditemukan.");
        await ValidateAsync(input, id, ct);
        Map(entity, input);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
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

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Locations.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Lokasi tidak ditemukan.");
        var used = await db.Locations.IgnoreQueryFilters().AnyAsync(x => x.ParentLocationId == id, ct)
            || await db.Stocks.IgnoreQueryFilters().AnyAsync(x => x.LocationId == id, ct)
            || await db.StockInHeaders.IgnoreQueryFilters().AnyAsync(x => x.LocationId == id, ct)
            || await db.StockOutHeaders.IgnoreQueryFilters().AnyAsync(x => x.LocationId == id, ct)
            || await db.StockTransferHeaders.IgnoreQueryFilters().AnyAsync(x => x.FromLocationId == id || x.ToLocationId == id, ct)
            || await db.StockAdjustmentHeaders.IgnoreQueryFilters().AnyAsync(x => x.LocationId == id, ct)
            || await db.StockOpnameHeaders.IgnoreQueryFilters().AnyAsync(x => x.LocationId == id, ct);
        if (used)
            throw new ValidationException("Lokasi tidak dapat dihapus karena masih dipakai stok/transaksi atau memiliki sub-lokasi. Non-aktifkan lokasi ini.");
        db.Locations.Remove(entity);
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

    private async Task ValidateAsync(LocationInput input, Guid? id, CancellationToken ct)
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

    private async Task<bool> CreatesCycleAsync(Guid currentId, Guid parentId, CancellationToken ct)
    {
        var visited = new HashSet<Guid>();
        var cursor = (Guid?)parentId;
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

    private static LocationResponse MapResponse(Location entity, string? parentName) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Type = entity.Type,
        ParentLocationId = entity.ParentLocationId,
        ParentLocationName = parentName,
        BranchCode = entity.BranchCode,
        Address = entity.Address,
        IsActive = entity.IsActive
    };

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
