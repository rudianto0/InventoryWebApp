using Inventory.Application.Dtos;
using Inventory.Application.Services;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize]
public sealed class CategoriesController(CatalogService catalog) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        ViewBag.Search = search;
        return View(await catalog.GetCategoriesAsync(search, ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View("Form", new CategoryFormModel());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormModel model, CancellationToken ct)
    {
        try
        {
            await catalog.CreateCategoryAsync(new CategoryInput
            {
                Name = model.Name, Description = model.Description, IsActive = model.IsActive
            }, ct);
            NotifySuccess("Kategori berhasil dibuat.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            return View("Form", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var entity = await catalog.GetCategoryAsync(id, ct);
        if (entity is null)
            return NotFound();
        return View("Form", new CategoryFormModel
        {
            Id = entity.Id, Name = entity.Name, Description = entity.Description, IsActive = entity.IsActive
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryFormModel model, CancellationToken ct)
    {
        try
        {
            await catalog.UpdateCategoryAsync(id, new CategoryInput
            {
                Name = model.Name, Description = model.Description, IsActive = model.IsActive
            }, ct);
            NotifySuccess("Kategori berhasil diperbarui.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            return View("Form", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await catalog.DeactivateCategoryAsync(id, ct);
            NotifySuccess("Kategori dinonaktifkan.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await catalog.DeleteCategoryAsync(id, ct);
            NotifySuccess("Kategori dihapus.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public sealed class UnitsController(CatalogService catalog) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        ViewBag.Search = search;
        return View(await catalog.GetUnitsAsync(search, ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View("Form", new UnitFormModel());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitFormModel model, CancellationToken ct)
    {
        try
        {
            await catalog.CreateUnitAsync(new UnitInput { Code = model.Code, Name = model.Name }, ct);
            NotifySuccess("Unit berhasil dibuat.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            return View("Form", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var entity = await catalog.GetUnitAsync(id, ct);
        if (entity is null)
            return NotFound();
        return View("Form", new UnitFormModel { Id = entity.Id, Code = entity.Code, Name = entity.Name });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UnitFormModel model, CancellationToken ct)
    {
        try
        {
            await catalog.UpdateUnitAsync(id, new UnitInput { Code = model.Code, Name = model.Name }, ct);
            NotifySuccess("Unit berhasil diperbarui.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            return View("Form", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await catalog.DeleteUnitAsync(id, ct);
            NotifySuccess("Unit dihapus.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public sealed class ProductsController(CatalogService catalog) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, Guid? categoryId, CancellationToken ct)
    {
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = await catalog.GetCategoryOptionsAsync(ct);
        return View(await catalog.GetProductsAsync(search, categoryId, null, ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct) =>
        View("Form", new ProductFormModel
        {
            Categories = await catalog.GetCategoryOptionsAsync(ct),
            Units = await catalog.GetUnitOptionsAsync(ct)
        });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormModel model, CancellationToken ct)
    {
        try
        {
            await catalog.CreateProductAsync(Map(model), ct);
            NotifySuccess("Produk berhasil dibuat.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Categories = await catalog.GetCategoryOptionsAsync(ct);
            model.Units = await catalog.GetUnitOptionsAsync(ct);
            return View("Form", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var entity = await catalog.GetProductAsync(id, ct);
        if (entity is null)
            return NotFound();
        return View("Form", new ProductFormModel
        {
            Id = entity.Id,
            Sku = entity.Sku,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            UnitId = entity.UnitId,
            ReorderLevel = entity.ReorderLevel,
            IsActive = entity.IsActive,
            Categories = await catalog.GetCategoryOptionsAsync(ct),
            Units = await catalog.GetUnitOptionsAsync(ct)
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductFormModel model, CancellationToken ct)
    {
        try
        {
            await catalog.UpdateProductAsync(id, Map(model), ct);
            NotifySuccess("Produk berhasil diperbarui.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Categories = await catalog.GetCategoryOptionsAsync(ct);
            model.Units = await catalog.GetUnitOptionsAsync(ct);
            return View("Form", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await catalog.DeactivateProductAsync(id, ct);
            NotifySuccess("Produk dinonaktifkan.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await catalog.DeleteProductAsync(id, ct);
            NotifySuccess("Produk dihapus.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }

    private static ProductInput Map(ProductFormModel model) => new()
    {
        Sku = model.Sku,
        Name = model.Name,
        CategoryId = model.CategoryId,
        UnitId = model.UnitId,
        ReorderLevel = model.ReorderLevel,
        IsActive = model.IsActive
    };
}
