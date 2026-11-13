using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Dtos;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class SupplierService(IAppDbContext db)
{
    public async Task<IReadOnlyList<Supplier>> ListAsync(string? search = null, bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = db.Suppliers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        if (activeOnly == true)
            query = query.Where(x => x.IsActive);
        return await query.OrderBy(x => x.Code).ToListAsync(ct);
    }

    public Task<Supplier?> GetAsync(int id, CancellationToken ct = default) =>
        db.Suppliers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Supplier> CreateAsync(SupplierInput input, CancellationToken ct = default)
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
        return entity;
    }

    public async Task UpdateAsync(int id, SupplierInput input, CancellationToken ct = default)
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

    public async Task DeactivateAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Supplier tidak ditemukan.");
        entity.IsActive = false;
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
