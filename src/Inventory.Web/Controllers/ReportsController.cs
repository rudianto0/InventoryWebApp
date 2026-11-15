using Inventory.Application.Dtos;
using Inventory.Application.Services;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize]
public sealed class DashboardController(ReportService reports, LocationService locations) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid? locationId, CancellationToken ct)
    {
        ViewBag.LocationId = locationId;
        ViewBag.Locations = await locations.GetOptionsAsync(ct: ct);
        return View(await reports.GetDashboardAsync(locationId, ct));
    }
}

[Authorize]
public sealed class ReportsController(
    ReportService reports,
    LocationService locations,
    CatalogService catalog) : AppControllerBase
{
    [HttpGet]
    public IActionResult Index() => RedirectToAction("Index", "Dashboard");

    [HttpGet]
    public async Task<IActionResult> StockBalance(ReportFilter filter, CancellationToken ct)
    {
        await PopulateOptionsAsync(ct);
        return View(new PagedReportModel<StockBalanceItem>
        {
            Title = "Laporan Saldo Stok",
            ReportKey = "stock-balance",
            Filter = filter,
            Result = await reports.GetStockBalanceAsync(filter, ct)
        });
    }

    [Authorize(Roles = "Admin,Manager,Auditor,SPV")]
    [HttpGet]
    public async Task<IActionResult> Movements(ReportFilter filter, CancellationToken ct)
    {
        await PopulateOptionsAsync(ct);
        return View(new PagedReportModel<StockMovementItem>
        {
            Title = "Laporan Mutasi Stok",
            ReportKey = "movements",
            Filter = filter,
            Result = await reports.GetMovementsAsync(filter, ct)
        });
    }

    [Authorize(Roles = "Admin,Manager,Auditor,SPV")]
    [HttpGet]
    public async Task<IActionResult> Reorder(ReportFilter filter, CancellationToken ct)
    {
        await PopulateOptionsAsync(ct);
        return View(new PagedReportModel<ReorderItem>
        {
            Title = "Reorder Alert",
            ReportKey = "reorder",
            Filter = filter,
            Result = await reports.GetReorderAsync(filter, ct)
        });
    }

    [HttpGet]
    public async Task<IActionResult> StockCard(ReportFilter filter, CancellationToken ct)
    {
        await PopulateOptionsAsync(ct);
        var items = await reports.GetStockCardAsync(filter, ct);
        return View(new StockCardPageModel { Filter = filter, Items = items });
    }

    [HttpGet]
    public async Task<IActionResult> Export(string report, ReportFilter filter, CancellationToken ct)
    {
        if (User.IsInRole("Staff") && !string.Equals(report, "stock-balance", StringComparison.OrdinalIgnoreCase))
            return Forbid();
        var csv = await reports.ExportCsvAsync(report, filter, ct);
        var name = $"{report}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", name);
    }

    private async Task PopulateOptionsAsync(CancellationToken ct)
    {
        ViewBag.Locations = await locations.GetOptionsAsync(activeOnly: false, ct);
        ViewBag.Products = await catalog.GetProductOptionsAsync(ct);
    }
}
