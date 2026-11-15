using Inventory.Contracts;
using Inventory.Client;
using Inventory.Contracts.Enums;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize]
public sealed class StockOpnamesController(
    IStockOpnameClient service,
    ILocationClient locations) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(ApprovalStatus? status, DateTime? from, DateTime? to, int page = 1, CancellationToken ct = default)
    {
        var filter = new TransactionFilter { Status = status, From = from, To = to, Page = page };
        return View("~/Views/Transactions/Index.cshtml", new TransactionIndexModel
        {
            Title = "Stock Opname (Cycle Count)",
            ControllerName = "StockOpnames",
            Result = await service.ListAsync(filter, ct),
            Filter = filter
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
            Title = "Stock Opname", ControllerName = "StockOpnames", Mode = "opname", Detail = detail
        });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var model = new TransactionFormModel
        {
            Title = "Buat Sesi Stock Opname",
            ControllerName = "StockOpnames",
            FormAction = "Create",
            Mode = "opname"
        };
        return View("~/Views/Transactions/Form.cshtml", await PopulateAsync(model, ct));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionFormModel model, CancellationToken ct)
    {
        try
        {
            var header = await service.CreateAsync(new StockOpnameInput
            {
                LocationId = model.LocationId,
                Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Local).ToUniversalTime(),
                Notes = model.Notes
            }, ct);
            NotifySuccess($"Sesi Stock Opname {header.DocNo} berhasil dibuat.");
            return RedirectToAction(nameof(Edit), new { id = header.Id });
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            model.Title = "Buat Sesi Stock Opname";
            model.ControllerName = "StockOpnames";
            model.Mode = "opname";
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
            Title = "Input Hasil Stock Opname",
            ControllerName = "StockOpnames",
            FormAction = "Edit",
            IsEdit = true,
            Mode = "opname",
            DocNo = detail.DocNo,
            LocationId = detail.LocationId,
            Date = detail.Date.ToLocalTime().Date,
            Notes = detail.Notes,
            OpnameLines = detail.Lines.Select(x => new OpnameLineRow
            {
                DetailId = x.DetailId,
                CountedQuantity = x.CountedQuantity,
                Notes = x.Notes
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
            await service.UpdateAsync(id, model.OpnameLines.Select(x => new OpnameLineInput
            {
                DetailId = x.DetailId, CountedQuantity = x.CountedQuantity, Notes = x.Notes
            }).ToList(), model.Notes, ct);
            NotifySuccess("Hasil hitung Stock Opname berhasil disimpan.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            var detail = await service.GetDetailAsync(id, ct);
            if (detail is null)
                return NotFound();
            model.Title = "Input Hasil Stock Opname";
            model.ControllerName = "StockOpnames";
            model.IsEdit = true;
            model.FormAction = "Edit";
            model.Mode = "opname";
            model.OpnameLines = detail.Lines.Select(x => new OpnameLineRow
            {
                DetailId = x.DetailId,
                CountedQuantity = x.CountedQuantity,
                Notes = x.Notes
            }).ToList();
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
            NotifySuccess("Stock Opname berhasil disubmit untuk approval.");
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
            NotifySuccess("Stock Opname disetujui dan saldo stok telah diterapkan.");
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
            NotifySuccess("Stock Opname ditolak.");
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
            NotifySuccess("Stock Opname dihapus.");
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
        model.Locations = await locations.GetOptionsAsync(ct: ct);
        return model;
    }
}
