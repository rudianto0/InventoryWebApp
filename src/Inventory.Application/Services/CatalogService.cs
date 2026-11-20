using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed class CatalogService(IAppDbContext db)
{
    public async Task<PagedResult<CategoryResponse>> GetCategoriesAsync(string? search = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default)
    {
        var query = db.Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var currentPage = Sorting.NormalizePage(page);
        var size = Sorting.NormalizePageSize(pageSize);
        var items = await SortCategories(query, Sorting.NormalizeKey(sort?.SortBy), sort?.SortDir ?? SortDirection.Asc)
            .Skip((currentPage - 1) * size)
            .Take(size)
            .Select(x => new CategoryResponse
            {
                Id = x.Id, Name = x.Name, Description = x.Description, IsActive = x.IsActive
            }).ToListAsync(ct);

        return new PagedResult<CategoryResponse>
        {
            Items = items,
            TotalCount = total,
            Page = currentPage,
            PageSize = size
        };
    }

    private static IOrderedQueryable<Category> SortCategories(IQueryable<Category> query, string? sortBy, SortDirection direction)
    {
        var desc = Sorting.IsDesc(direction);
        IOrderedQueryable<Category> ordered = (sortBy, desc) switch
        {
            ("name", true) => query.OrderByDescending(x => x.Name.ToLower()),
            ("description", false) => query.OrderBy(x => x.Description == null).ThenBy(x => x.Description!.ToLower()),
            ("description", true) => query.OrderBy(x => x.Description == null).ThenByDescending(x => x.Description!.ToLower()),
            ("status", false) => query.OrderBy(x => x.IsActive),
            ("status", true) => query.OrderByDescending(x => x.IsActive),
            _ => query.OrderBy(x => x.Name.ToLower())
        };
        return ordered.ThenBy(x => x.Id);
    }

    public Task<CategoryResponse?> GetCategoryAsync(Guid id, CancellationToken ct = default) =>
        db.Categories.Where(x => x.Id == id).Select(x => new CategoryResponse
        {
            Id = x.Id, Name = x.Name, Description = x.Description, IsActive = x.IsActive
        }).FirstOrDefaultAsync(ct);

    public async Task<CategoryResponse> CreateCategoryAsync(CategoryInput input, CancellationToken ct = default)
    {
        Guard.Required(input.Name, "Nama kategori");
        var name = input.Name.Trim();
        if (await db.Categories.AnyAsync(x => x.Name == name, ct))
            throw new ValidationException("Nama kategori sudah digunakan.");

        var entity = new Category { Name = name, Description = Guard.TrimOrNull(input.Description), IsActive = input.IsActive };
        db.Categories.Add(entity);
        await db.SaveChangesAsync(ct);
        return new CategoryResponse
        {
            Id = entity.Id, Name = entity.Name, Description = entity.Description,
            IsActive = entity.IsActive, CreatedById = entity.CreatedById
        };
    }

    public async Task UpdateCategoryAsync(Guid id, CategoryInput input, CancellationToken ct = default)
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

    public async Task DeactivateCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Kategori tidak ditemukan.");
        if (await db.Products.AnyAsync(x => x.CategoryId == id && x.IsActive, ct))
            throw new ValidationException("Kategori masih dipakai produk aktif. Non-aktifkan produk terlebih dahulu.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Kategori tidak ditemukan.");
        if (await db.Products.IgnoreQueryFilters().AnyAsync(x => x.CategoryId == id, ct))
            throw new ValidationException("Kategori tidak dapat dihapus karena masih dipakai produk. Non-aktifkan kategori ini.");
        db.Categories.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<UnitResponse>> GetUnitsAsync(string? search = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default)
    {
        var query = db.Units.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var currentPage = Sorting.NormalizePage(page);
        var size = Sorting.NormalizePageSize(pageSize);
        var items = await SortUnits(query, Sorting.NormalizeKey(sort?.SortBy), sort?.SortDir ?? SortDirection.Asc)
            .Skip((currentPage - 1) * size)
            .Take(size)
            .Select(x => new UnitResponse
            {
                Id = x.Id, Code = x.Code, Name = x.Name
            }).ToListAsync(ct);

        return new PagedResult<UnitResponse>
        {
            Items = items,
            TotalCount = total,
            Page = currentPage,
            PageSize = size
        };
    }

    private static IOrderedQueryable<Unit> SortUnits(IQueryable<Unit> query, string? sortBy, SortDirection direction)
    {
        var desc = Sorting.IsDesc(direction);
        IOrderedQueryable<Unit> ordered = (sortBy, desc) switch
        {
            ("code", true) => query.OrderByDescending(x => x.Code.ToLower()),
            ("name", false) => query.OrderBy(x => x.Name.ToLower()),
            ("name", true) => query.OrderByDescending(x => x.Name.ToLower()),
            _ => query.OrderBy(x => x.Code.ToLower())
        };
        return ordered.ThenBy(x => x.Id);
    }

    public Task<UnitResponse?> GetUnitAsync(Guid id, CancellationToken ct = default) =>
        db.Units.Where(x => x.Id == id).Select(x => new UnitResponse
        {
            Id = x.Id, Code = x.Code, Name = x.Name
        }).FirstOrDefaultAsync(ct);

    public async Task<UnitResponse> CreateUnitAsync(UnitInput input, CancellationToken ct = default)
    {
        Guard.Required(input.Code, "Kode unit");
        Guard.Required(input.Name, "Nama unit");
        var code = input.Code.Trim().ToUpperInvariant();
        if (await db.Units.AnyAsync(x => x.Code == code, ct))
            throw new ValidationException("Kode unit sudah digunakan.");

        var entity = new Unit { Code = code, Name = input.Name.Trim() };
        db.Units.Add(entity);
        await db.SaveChangesAsync(ct);
        return new UnitResponse { Id = entity.Id, Code = entity.Code, Name = entity.Name };
    }

    public async Task UpdateUnitAsync(Guid id, UnitInput input, CancellationToken ct = default)
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

    public async Task DeleteUnitAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Units.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Unit tidak ditemukan.");
        if (await db.Products.IgnoreQueryFilters().AnyAsync(x => x.UnitId == id, ct))
            throw new ValidationException("Unit tidak dapat dihapus karena masih dipakai produk.");
        db.Units.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<ProductListItem>> GetProductsAsync(string? search = null, Guid? categoryId = null, bool? activeOnly = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default)
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

        var total = await query.CountAsync(ct);
        var currentPage = Sorting.NormalizePage(page);
        var size = Sorting.NormalizePageSize(pageSize);
        var items = await SortProducts(query, Sorting.NormalizeKey(sort?.SortBy), sort?.SortDir ?? SortDirection.Asc)
            .Skip((currentPage - 1) * size)
            .Take(size)
            .Select(x => new ProductListItem
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                CategoryName = x.Category!.Name,
                UnitName = x.Unit!.Name,
                ReorderLevel = x.ReorderLevel,
                IsActive = x.IsActive
            }).ToListAsync(ct);

        return new PagedResult<ProductListItem>
        {
            Items = items,
            TotalCount = total,
            Page = currentPage,
            PageSize = size
        };
    }

    private static IOrderedQueryable<Product> SortProducts(IQueryable<Product> query, string? sortBy, SortDirection direction)
    {
        var desc = Sorting.IsDesc(direction);
        IOrderedQueryable<Product> ordered = (sortBy, desc) switch
        {
            ("sku", true) => query.OrderByDescending(x => x.Sku.ToLower()),
            ("name", false) => query.OrderBy(x => x.Name.ToLower()),
            ("name", true) => query.OrderByDescending(x => x.Name.ToLower()),
            ("category", false) => query.OrderBy(x => x.Category!.Name.ToLower()),
            ("category", true) => query.OrderByDescending(x => x.Category!.Name.ToLower()),
            ("unit", false) => query.OrderBy(x => x.Unit!.Name.ToLower()),
            ("unit", true) => query.OrderByDescending(x => x.Unit!.Name.ToLower()),
            ("reorder", false) => query.OrderBy(x => x.ReorderLevel),
            ("reorder", true) => query.OrderByDescending(x => x.ReorderLevel),
            ("status", false) => query.OrderBy(x => x.IsActive),
            ("status", true) => query.OrderByDescending(x => x.IsActive),
            _ => query.OrderBy(x => x.Sku.ToLower())
        };
        return ordered.ThenBy(x => x.Id);
    }

    public Task<ProductResponse?> GetProductAsync(Guid id, CancellationToken ct = default) =>
        db.Products.Where(x => x.Id == id).Select(x => new ProductResponse
        {
            Id = x.Id, Sku = x.Sku, Name = x.Name, CategoryId = x.CategoryId,
            UnitId = x.UnitId, ReorderLevel = x.ReorderLevel, IsActive = x.IsActive, CreatedAt = x.CreatedAt
        }).FirstOrDefaultAsync(ct);

    public async Task<ProductResponse> CreateProductAsync(ProductInput input, CancellationToken ct = default)
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
        return new ProductResponse
        {
            Id = entity.Id, Sku = entity.Sku, Name = entity.Name, CategoryId = entity.CategoryId,
            UnitId = entity.UnitId, ReorderLevel = entity.ReorderLevel, IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task UpdateProductAsync(Guid id, ProductInput input, CancellationToken ct = default)
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

    public async Task DeactivateProductAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Produk tidak ditemukan.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Produk tidak ditemukan.");

        var used = await db.Stocks.IgnoreQueryFilters().AnyAsync(x => x.ProductId == id, ct)
            || await db.StockMovements.IgnoreQueryFilters().AnyAsync(x => x.ProductId == id, ct)
            || await db.StockInDetails.IgnoreQueryFilters().AnyAsync(x => x.ProductId == id, ct)
            || await db.StockOutDetails.IgnoreQueryFilters().AnyAsync(x => x.ProductId == id, ct)
            || await db.StockTransferDetails.IgnoreQueryFilters().AnyAsync(x => x.ProductId == id, ct)
            || await db.StockAdjustmentDetails.IgnoreQueryFilters().AnyAsync(x => x.ProductId == id, ct)
            || await db.StockOpnameDetails.IgnoreQueryFilters().AnyAsync(x => x.ProductId == id, ct);
        if (used)
            throw new ValidationException("Produk tidak dapat dihapus karena sudah memiliki riwayat stok/transaksi. Non-aktifkan produk ini.");

        db.Products.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidateProductInputAsync(ProductInput input, Guid? id, CancellationToken ct)
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
