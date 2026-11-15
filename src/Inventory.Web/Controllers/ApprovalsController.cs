using Inventory.Contracts;
using Inventory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "Admin,SPV,Manager")]
public sealed class ApprovalsController(
    IApprovalClient approvals,
    IStockInClient stockIns,
    IStockOutClient stockOuts,
    IStockTransferClient transfers,
    IStockAdjustmentClient adjustments,
    IStockOpnameClient opnames) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await approvals.GetPendingAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, string type, string? notes, CancellationToken ct)
    {
        try
        {
            await ExecuteApproveAsync(id, type, notes, ct);
            NotifySuccess("Dokumen berhasil disetujui.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, string type, string reason, CancellationToken ct)
    {
        try
        {
            await ExecuteRejectAsync(id, type, reason, ct);
            NotifySuccess("Dokumen ditolak.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task ExecuteApproveAsync(Guid id, string type, string? notes, CancellationToken ct)
    {
        switch (type)
        {
            case "StockIn": await stockIns.ApproveAsync(id, notes, ct); break;
            case "StockOut": await stockOuts.ApproveAsync(id, notes, ct); break;
            case "StockTransfer": await transfers.ApproveAsync(id, notes, ct); break;
            case "StockAdjustment": await adjustments.ApproveAsync(id, notes, ct); break;
            case "StockOpname": await opnames.ApproveAsync(id, notes, ct); break;
            default: throw new InvalidOperationException("Tipe dokumen tidak dikenal.");
        }
    }

    private async Task ExecuteRejectAsync(Guid id, string type, string reason, CancellationToken ct)
    {
        switch (type)
        {
            case "StockIn": await stockIns.RejectAsync(id, reason, ct); break;
            case "StockOut": await stockOuts.RejectAsync(id, reason, ct); break;
            case "StockTransfer": await transfers.RejectAsync(id, reason, ct); break;
            case "StockAdjustment": await adjustments.RejectAsync(id, reason, ct); break;
            case "StockOpname": await opnames.RejectAsync(id, reason, ct); break;
            default: throw new InvalidOperationException("Tipe dokumen tidak dikenal.");
        }
    }
}
