using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class SupplierService(IAppDbContext db)
{
    public async Task<PagedResult<SupplierResponse>> ListAsync(string? search = null, bool? activeOnly = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default)
    {
        var query = db.Suppliers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        if (activeOnly == true)
            query = query.Where(x => x.IsActive);

        var total = await query.CountAsync(ct);
        var currentPage = Sorting.NormalizePage(page);
        var size = Sorting.NormalizePageSize(pageSize);
        var items = await SortSuppliers(query, Sorting.NormalizeKey(sort?.SortBy), sort?.SortDir ?? SortDirection.Asc)
            .Skip((currentPage - 1) * size)
            .Take(size)
            .Select(x => new SupplierResponse
            {
                Id = x.Id, Code = x.Code, Name = x.Name, ContactPerson = x.ContactPerson,
                Phone = x.Phone, Email = x.Email, Address = x.Address, IsActive = x.IsActive
            }).ToListAsync(ct);

        return new PagedResult<SupplierResponse>
        {
            Items = items,
            TotalCount = total,
            Page = currentPage,
            PageSize = size
        };
    }

    private static IOrderedQueryable<Supplier> SortSuppliers(IQueryable<Supplier> query, string? sortBy, SortDirection direction)
    {
        var desc = Sorting.IsDesc(direction);
        IOrderedQueryable<Supplier> ordered = (sortBy, desc) switch
        {
            ("code", true) => query.OrderByDescending(x => x.Code.ToLower()),
            ("name", false) => query.OrderBy(x => x.Name.ToLower()),
            ("name", true) => query.OrderByDescending(x => x.Name.ToLower()),
            ("contact", false) => query.OrderBy(x => x.ContactPerson == null).ThenBy(x => x.ContactPerson!.ToLower()),
            ("contact", true) => query.OrderBy(x => x.ContactPerson == null).ThenByDescending(x => x.ContactPerson!.ToLower()),
            ("phone", false) => query.OrderBy(x => x.Phone == null).ThenBy(x => x.Phone!.ToLower()),
            ("phone", true) => query.OrderBy(x => x.Phone == null).ThenByDescending(x => x.Phone!.ToLower()),
            ("email", false) => query.OrderBy(x => x.Email == null).ThenBy(x => x.Email!.ToLower()),
            ("email", true) => query.OrderBy(x => x.Email == null).ThenByDescending(x => x.Email!.ToLower()),
            ("status", false) => query.OrderBy(x => x.IsActive),
            ("status", true) => query.OrderByDescending(x => x.IsActive),
            _ => query.OrderBy(x => x.Code.ToLower())
        };
        return ordered.ThenBy(x => x.Id);
    }

    public Task<SupplierResponse?> GetAsync(Guid id, CancellationToken ct = default) =>
        db.Suppliers.Where(x => x.Id == id).Select(x => new SupplierResponse
        {
            Id = x.Id, Code = x.Code, Name = x.Name, ContactPerson = x.ContactPerson,
            Phone = x.Phone, Email = x.Email, Address = x.Address, IsActive = x.IsActive
        }).FirstOrDefaultAsync(ct);

    public async Task<SupplierResponse> CreateAsync(SupplierInput input, CancellationToken ct = default)
    {
        Guard.Required(input.Code, "Kode supplier");
        Guard.Required(input.Name, "Nama supplier");
        var code = input.Code.Trim().ToUpperInvariant();
        if (await db.Suppliers.AnyAsync(x => x.Code == code, ct))
            throw new ValidationException("Kode supplier sudah digunakan.");
        ValidateEmail(input.Email);

        var entity = new Supplier
        {
            Code = code,
            Name = input.Name.Trim(),
            ContactPerson = Guard.TrimOrNull(input.ContactPerson),
            Phone = Guard.TrimOrNull(input.Phone),
            Email = Guard.TrimOrNull(input.Email),
            Address = Guard.TrimOrNull(input.Address),
            IsActive = input.IsActive
        };
        db.Suppliers.Add(entity);
        await db.SaveChangesAsync(ct);
        return new SupplierResponse
        {
            Id = entity.Id, Code = entity.Code, Name = entity.Name, ContactPerson = entity.ContactPerson,
            Phone = entity.Phone, Email = entity.Email, Address = entity.Address, IsActive = entity.IsActive
        };
    }

    public async Task UpdateAsync(Guid id, SupplierInput input, CancellationToken ct = default)
    {
        Guard.Required(input.Code, "Kode supplier");
        Guard.Required(input.Name, "Nama supplier");
        var entity = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Supplier tidak ditemukan.");
        var code = input.Code.Trim().ToUpperInvariant();
        if (await db.Suppliers.AnyAsync(x => x.Code == code && x.Id != id, ct))
            throw new ValidationException("Kode supplier sudah digunakan.");
        ValidateEmail(input.Email);

        entity.Code = code;
        entity.Name = input.Name.Trim();
        entity.ContactPerson = Guard.TrimOrNull(input.ContactPerson);
        entity.Phone = Guard.TrimOrNull(input.Phone);
        entity.Email = Guard.TrimOrNull(input.Email);
        entity.Address = Guard.TrimOrNull(input.Address);
        entity.IsActive = input.IsActive;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Supplier tidak ditemukan.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Supplier tidak ditemukan.");
        if (await db.StockInHeaders.IgnoreQueryFilters().AnyAsync(x => x.SupplierId == id, ct))
            throw new ValidationException("Supplier tidak dapat dihapus karena sudah dipakai transaksi Stock In. Non-aktifkan supplier ini.");
        db.Suppliers.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SelectOption>> GetOptionsAsync(CancellationToken ct = default) =>
        await db.Suppliers.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code)
            .Select(x => new SelectOption { Id = x.Id, Text = x.Code + " - " + x.Name }).ToListAsync(ct);

    private static void ValidateEmail(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email) && !new EmailAddressAttribute().IsValid(email))
            throw new ValidationException("Format email tidak valid.");
    }
}
