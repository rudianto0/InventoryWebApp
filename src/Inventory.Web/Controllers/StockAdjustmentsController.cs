using Inventory.Contracts;
using Inventory.Client;
using Inventory.Contracts.Enums;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize]
public sealed class StockAdjustmentsController(
    IStockAdjustmentClient service,
    ICatalogClient catalog,
    ILocationClient locations) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(ApprovalStatus? status, DateTime? from, DateTime? to, string? sortBy, SortDirection sortDir, int page = 1, CancellationToken ct = default)
    {
        var filter = new TransactionFilter { Status = status, From = from, To = to, SortBy = sortBy, SortDir = sortDir, Page = page };
        return View("~/Views/Transactions/Index.cshtml", new TransactionIndexModel
        {
            Title = "Stock Adjustment",
            ControllerName = "StockAdjustments",
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
            Title = "Stock Adjustment", ControllerName = "StockAdjustments", Mode = "adjustment", Detail = detail
        });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var model = new TransactionFormModel
        {
            Title = "Buat Stock Adjustment", ControllerName = "StockAdjustments", FormAction = "Create",
            Mode = "adjustment", ShowReason = true,
            AdjustmentLines = { new AdjustmentLineRow() }
        };
        return View("~/Views/Transactions/Form.cshtml", await PopulateAsync(model, ct));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionFormModel model, CancellationToken ct)
    {
        if (model.AdjustmentLines.All(x => x.ProductId == Guid.Empty))
            ModelState.AddModelError(string.Empty, "Minimal satu baris produk harus diisi.");
        if (!ModelState.IsValid)
        {
            model.Title = "Buat Stock Adjustment";
            model.ControllerName = "StockAdjustments";
            model.Mode = "adjustment";
            return View("~/Views/Transactions/Form.cshtml", await PopulateAsync(model, ct));
        }

        try
        {
            var header = await service.CreateAsync(Map(model), ct);
            NotifySuccess($"Stock Adjustment {header.DocNo} berhasil dibuat.");
            return RedirectToAction(nameof(Details), new { id = header.Id });
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Title = "Buat Stock Adjustment";
            model.ControllerName = "StockAdjustments";
            model.Mode = "adjustment";
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
            Title = "Edit Stock Adjustment",
            ControllerName = "StockAdjustments",
            FormAction = "Edit",
            IsEdit = true,
            Mode = "adjustment",
            ShowReason = true,
            DocNo = detail.DocNo,
            LocationId = detail.LocationId,
            Date = detail.Date.ToLocalTime().Date,
            Reason = detail.Reason,
            Notes = detail.Notes,
            AdjustmentLines = detail.Lines.Select(x => new AdjustmentLineRow
            {
                ProductId = x.ProductId, CountedQuantity = x.CountedQuantity ?? 0, Notes = x.Notes
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
            NotifySuccess("Stock Adjustment berhasil diperbarui.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Title = "Edit Stock Adjustment";
            model.ControllerName = "StockAdjustments";
            model.IsEdit = true;
            model.FormAction = "Edit";
            model.Mode = "adjustment";
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
            NotifySuccess("Stock Adjustment berhasil disubmit untuk approval.");
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
            NotifySuccess("Stock Adjustment disetujui dan stok telah disesuaikan.");
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
            NotifySuccess("Stock Adjustment ditolak.");
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
            NotifySuccess("Stock Adjustment dihapus.");
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
        if (model.AdjustmentLines.Count == 0)
            model.AdjustmentLines.Add(new AdjustmentLineRow());
        return model;
    }

    private static StockAdjustmentInput Map(TransactionFormModel model) => new()
    {
        LocationId = model.LocationId,
        Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Local).ToUniversalTime(),
        Reason = model.Reason ?? string.Empty,
        Notes = model.Notes,
        Lines = model.AdjustmentLines.Where(x => x.ProductId != Guid.Empty).Select(x => new AdjustmentLineInput
        {
            ProductId = x.ProductId, CountedQuantity = x.CountedQuantity, Notes = x.Notes
        }).ToList()
    };
}
