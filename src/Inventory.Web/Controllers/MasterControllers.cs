using Inventory.Application.Dtos;
using Inventory.Application.Services;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize]
public sealed class LocationsController(LocationService locations) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(Domain.Enums.LocationType? type, CancellationToken ct)
    {
        ViewBag.Type = type;
        return View(await locations.ListAsync(type, null, ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var model = new LocationFormModel { Parents = await GetParentsAsync(ct) };
        return View("Form", model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationFormModel model, CancellationToken ct)
    {
        try
        {
            await locations.CreateAsync(Map(model), ct);
            NotifySuccess("Lokasi berhasil dibuat.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Parents = await GetParentsAsync(ct);
            return View("Form", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var entity = await locations.GetAsync(id, ct);
        if (entity is null)
            return NotFound();
        return View("Form", new LocationFormModel
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Type = entity.Type,
            ParentLocationId = entity.ParentLocationId,
            BranchCode = entity.BranchCode,
            Address = entity.Address,
            IsActive = entity.IsActive,
            Parents = await GetParentsAsync(ct, entity.Id)
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LocationFormModel model, CancellationToken ct)
    {
        try
        {
            await locations.UpdateAsync(id, Map(model), ct);
            NotifySuccess("Lokasi berhasil diperbarui.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Parents = await GetParentsAsync(ct, id);
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
            await locations.DeactivateAsync(id, ct);
            NotifySuccess("Lokasi dinonaktifkan.");
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
            await locations.DeleteAsync(id, ct);
            NotifySuccess("Lokasi dihapus.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IReadOnlyList<SelectOption>> GetParentsAsync(CancellationToken ct, Guid? excludeId = null)
    {
        var all = await locations.GetOptionsAsync(activeOnly: false, ct);
        return excludeId.HasValue ? all.Where(x => x.Id != excludeId.Value).ToList() : all;
    }

    private static LocationInput Map(LocationFormModel model) => new()
    {
        Code = model.Code,
        Name = model.Name,
        Type = model.Type,
        ParentLocationId = model.ParentLocationId,
        BranchCode = model.BranchCode,
        Address = model.Address,
        IsActive = model.IsActive
    };
}

[Authorize]
public sealed class SuppliersController(SupplierService suppliers) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        ViewBag.Search = search;
        return View(await suppliers.ListAsync(search, null, ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View("Form", new SupplierFormModel());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierFormModel model, CancellationToken ct)
    {
        try
        {
            await suppliers.CreateAsync(Map(model), ct);
            NotifySuccess("Supplier berhasil dibuat.");
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
        var entity = await suppliers.GetAsync(id, ct);
        if (entity is null)
            return NotFound();
        return View("Form", new SupplierFormModel
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ContactPerson = entity.ContactPerson,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            IsActive = entity.IsActive
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SupplierFormModel model, CancellationToken ct)
    {
        try
        {
            await suppliers.UpdateAsync(id, Map(model), ct);
            NotifySuccess("Supplier berhasil diperbarui.");
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
            await suppliers.DeactivateAsync(id, ct);
            NotifySuccess("Supplier dinonaktifkan.");
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
            await suppliers.DeleteAsync(id, ct);
            NotifySuccess("Supplier dihapus.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }

    private static SupplierInput Map(SupplierFormModel model) => new()
    {
        Code = model.Code,
        Name = model.Name,
        ContactPerson = model.ContactPerson,
        Phone = model.Phone,
        Email = model.Email,
        Address = model.Address,
        IsActive = model.IsActive
    };
}
