using Inventory.Contracts;
using Inventory.Client;
using Inventory.Contracts.Enums;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize]
public sealed class StockInsController(
    IStockInClient service,
    ICatalogClient catalog,
    ISupplierClient suppliers,
    ILocationClient locations) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(ApprovalStatus? status, DateTime? from, DateTime? to, string? sortBy, SortDirection sortDir, int page = 1, CancellationToken ct = default)
    {
        var filter = new TransactionFilter { Status = status, From = from, To = to, SortBy = sortBy, SortDir = sortDir, Page = page };
        return View("~/Views/Transactions/Index.cshtml", new TransactionIndexModel
        {
            Title = "Stock In (Receiving)",
            ControllerName = "StockIns",
            Result = await service.ListAsync(filter, ct),
            Filter = filter,
            ShowPartner = true
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var detail = await service.GetDetailAsync(id, ct);
        if (detail is null)
            return NotFound();
        return View("~/Views/Transactions/Details.cshtml", new TransactionDetailsModel
        {
            Title = "Stock In", ControllerName = "StockIns", Detail = detail
        });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var model = new TransactionFormModel
        {
            Title = "Buat Stock In", ControllerName = "StockIns", FormAction = "Create",
            ShowSupplier = true, ShowUnitCost = true, Lines = { new TransactionLineRow() }
        };
        return View("~/Views/Transactions/Form.cshtml", await PopulateAsync(model, ct));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionFormModel model, CancellationToken ct)
    {
        if (model.Lines.All(x => x.ProductId == Guid.Empty))
            ModelState.AddModelError(string.Empty, "Minimal satu baris produk harus diisi.");
        if (!ModelState.IsValid)
        {
            model.Title = "Buat Stock In";
            model.ControllerName = "StockIns";
            return View("~/Views/Transactions/Form.cshtml", await PopulateAsync(model, ct));
        }

        try
        {
            var header = await service.CreateAsync(Map(model), ct);
            NotifySuccess($"Stock In {header.DocNo} berhasil dibuat.");
            return RedirectToAction(nameof(Details), new { id = header.Id });
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Title = "Buat Stock In";
            model.ControllerName = "StockIns";
            return View("~/Views/Transactions/Form.cshtml", await PopulateAsync(model, ct));
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var detail = await service.GetDetailAsync(id, ct);
        if (detail is null)
            return NotFound();
        var model = new TransactionFormModel
        {
            Id = detail.Id,
            Title = "Edit Stock In",
            ControllerName = "StockIns",
            FormAction = "Edit",
            IsEdit = true,
            ShowSupplier = true,
            ShowUnitCost = true,
            DocNo = detail.DocNo,
            SupplierId = detail.SupplierId,
            LocationId = detail.LocationId,
            Date = detail.Date.ToLocalTime().Date,
            Notes = detail.Notes,
            Lines = detail.Lines.Select(x => new TransactionLineRow
            {
                ProductId = x.ProductId, Quantity = x.Quantity, UnitCost = x.UnitCost, Notes = x.Notes
            }).ToList()
        };
        return View("~/Views/Transactions/Form.cshtml", await PopulateAsync(model, ct));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TransactionFormModel model, CancellationToken ct)
    {
        try
        {
            await service.UpdateAsync(id, Map(model), ct);
            NotifySuccess("Stock In berhasil diperbarui.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Title = "Edit Stock In";
            model.ControllerName = "StockIns";
            model.IsEdit = true;
            model.FormAction = "Edit";
            return View("~/Views/Transactions/Form.cshtml", await PopulateAsync(model, ct));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        try
        {
            await service.SubmitAsync(id, ct);
            NotifySuccess("Stock In berhasil disubmit untuk approval.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, string? notes, CancellationToken ct)
    {
        try
        {
            await service.ApproveAsync(id, notes, ct);
            NotifySuccess("Stock In disetujui dan stok telah diperbarui.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, string reason, CancellationToken ct)
    {
        try
        {
            await service.RejectAsync(id, reason, ct);
            NotifySuccess("Stock In ditolak.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await service.DeleteAsync(id, ct);
            NotifySuccess("Stock In dihapus.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task<TransactionFormModel> PopulateAsync(TransactionFormModel model, CancellationToken ct)
    {
        model.Products = await catalog.GetProductOptionsAsync(ct);
        model.Locations = await locations.GetOptionsAsync(ct: ct);
        model.Suppliers = await suppliers.GetOptionsAsync(ct);
        if (model.Lines.Count == 0)
            model.Lines.Add(new TransactionLineRow());
        return model;
    }

    private static StockInInput Map(TransactionFormModel model) => new()
    {
        SupplierId = model.SupplierId ?? Guid.Empty,
        LocationId = model.LocationId,
        Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Local).ToUniversalTime(),
        Notes = model.Notes,
        Lines = model.Lines.Where(x => x.ProductId != Guid.Empty).Select(x => new DocumentLineInput
        {
            ProductId = x.ProductId, Quantity = x.Quantity, UnitCost = x.UnitCost, Notes = x.Notes
        }).ToList()
    };
}
