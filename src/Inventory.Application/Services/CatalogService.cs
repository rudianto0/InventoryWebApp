using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Dtos;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class CatalogService(IAppDbContext db)
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(string? search = null, CancellationToken ct = default)
    {
        var query = db.Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }
        return await query.OrderBy(x => x.Name).ToListAsync(ct);
    }

    public Task<Category?> GetCategoryAsync(int id, CancellationToken ct = default) =>
        db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Category> CreateCategoryAsync(CategoryInput input, CancellationToken ct = default)
    {
        Guard.Required(input.Name, "Nama kategori");
        var name = input.Name.Trim();
        if (await db.Categories.AnyAsync(x => x.Name == name, ct))
            throw new ValidationException("Nama kategori sudah digunakan.");

        var entity = new Category { Name = name, Description = Guard.TrimOrNull(input.Description), IsActive = input.IsActive };
        db.Categories.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateCategoryAsync(int id, CategoryInput input, CancellationToken ct = default)
    {
        Guard.Required(input.Name, "Nama kategori");
        var entity = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Kategori tidak ditemukan.");
        var name = input.Name.Trim();
        if (await db.Categories.AnyAsync(x => x.Name == name && x.Id != id, ct))
            throw new ValidationException("Nama kategori sudah digunakan.");

        entity.Name = name;
        entity.Description = Guard.TrimOrNull(input.Description);
        entity.IsActive = input.IsActive;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateCategoryAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Kategori tidak ditemukan.");
        if (await db.Products.AnyAsync(x => x.CategoryId == id && x.IsActive, ct))
            throw new ValidationException("Kategori masih dipakai produk aktif. Non-aktifkan produk terlebih dahulu.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Unit>> GetUnitsAsync(string? search = null, CancellationToken ct = default)
    {
        var query = db.Units.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        return await query.OrderBy(x => x.Code).ToListAsync(ct);
    }

    public Task<Unit?> GetUnitAsync(int id, CancellationToken ct = default) =>
        db.Units.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Unit> CreateUnitAsync(UnitInput input, CancellationToken ct = default)
    {
        Guard.Required(input.Code, "Kode unit");
        Guard.Required(input.Name, "Nama unit");
        var code = input.Code.Trim().ToUpperInvariant();
        if (await db.Units.AnyAsync(x => x.Code == code, ct))
            throw new ValidationException("Kode unit sudah digunakan.");

        var entity = new Unit { Code = code, Name = input.Name.Trim() };
        db.Units.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateUnitAsync(int id, UnitInput input, CancellationToken ct = default)
    {
        Guard.Required(input.Code, "Kode unit");
        Guard.Required(input.Name, "Nama unit");
        var entity = await db.Units.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Unit tidak ditemukan.");
        var code = input.Code.Trim().ToUpperInvariant();
        if (await db.Units.AnyAsync(x => x.Code == code && x.Id != id, ct))
            throw new ValidationException("Kode unit sudah digunakan.");

        entity.Code = code;
        entity.Name = input.Name.Trim();
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProductListItem>> GetProductsAsync(string? search = null, int? categoryId = null, bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Unit).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Sku.Contains(term) || x.Name.Contains(term));
        }
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);
        if (activeOnly == true)
            query = query.Where(x => x.IsActive);

        return await query.OrderBy(x => x.Sku).Select(x => new ProductListItem
        {
            Id = x.Id,
            Sku = x.Sku,
            Name = x.Name,
            CategoryName = x.Category!.Name,
            UnitName = x.Unit!.Name,
            ReorderLevel = x.ReorderLevel,
            IsActive = x.IsActive
        }).ToListAsync(ct);
    }

    public Task<Product?> GetProductAsync(int id, CancellationToken ct = default) =>
        db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Product> CreateProductAsync(ProductInput input, CancellationToken ct = default)
    {
        await ValidateProductInputAsync(input, null, ct);
        var entity = new Product
        {
            Sku = input.Sku.Trim().ToUpperInvariant(),
            Name = input.Name.Trim(),
            CategoryId = input.CategoryId,
            UnitId = input.UnitId,
            ReorderLevel = input.ReorderLevel,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        db.Products.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateProductAsync(int id, ProductInput input, CancellationToken ct = default)
    {
        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Produk tidak ditemukan.");
        await ValidateProductInputAsync(input, id, ct);

        entity.Sku = input.Sku.Trim().ToUpperInvariant();
        entity.Name = input.Name.Trim();
        entity.CategoryId = input.CategoryId;
        entity.UnitId = input.UnitId;
        entity.ReorderLevel = input.ReorderLevel;
        entity.IsActive = input.IsActive;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeactivateProductAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Produk tidak ditemukan.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidateProductInputAsync(ProductInput input, int? id, CancellationToken ct)
    {
        Guard.Required(input.Sku, "SKU");
        Guard.Required(input.Name, "Nama produk");
        if (input.ReorderLevel < 0)
            throw new ValidationException("Reorder level tidak boleh negatif.");

        var sku = input.Sku.Trim().ToUpperInvariant();
        if (sku.Contains(' '))
            throw new ValidationException("SKU tidak boleh mengandung spasi.");
        if (await db.Products.AnyAsync(x => x.Sku == sku && (!id.HasValue || x.Id != id.Value), ct))
            throw new ValidationException("SKU sudah digunakan.");

        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == input.CategoryId, ct)
            ?? throw new ValidationException("Kategori tidak ditemukan.");
        if (!category.IsActive)
            throw new ValidationException("Kategori tidak aktif.");

        var unit = await db.Units.FirstOrDefaultAsync(x => x.Id == input.UnitId, ct)
            ?? throw new ValidationException("Unit tidak ditemukan.");
        if (string.IsNullOrWhiteSpace(unit.Name))
            throw new ValidationException("Unit tidak valid.");
    }

    public async Task<IReadOnlyList<SelectOption>> GetCategoryOptionsAsync(CancellationToken ct = default) =>
        await db.Categories.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new SelectOption { Id = x.Id, Text = x.Name, IsActive = x.IsActive }).ToListAsync(ct);

    public async Task<IReadOnlyList<SelectOption>> GetUnitOptionsAsync(CancellationToken ct = default) =>
        await db.Units.AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new SelectOption { Id = x.Id, Text = x.Code + " - " + x.Name, IsActive = true }).ToListAsync(ct);

    public async Task<IReadOnlyList<SelectOption>> GetProductOptionsAsync(CancellationToken ct = default) =>
        await db.Products.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Sku)
            .Select(x => new SelectOption { Id = x.Id, Text = x.Sku + " - " + x.Name, IsActive = true }).ToListAsync(ct);
}
