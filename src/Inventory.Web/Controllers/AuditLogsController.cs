using Inventory.Contracts;
using Inventory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "Admin,Auditor")]
public sealed class AuditLogsController(IAuditClient audit) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(AuditFilter filter, CancellationToken ct)
    {
        ViewBag.EntityNames = await audit.GetEntityNamesAsync(ct);
        ViewBag.Actions = new[] { "Create", "Update", "Delete", "Submit", "Approve", "Reject" };
        ViewBag.Filter = filter;
        return View(await audit.ListAsync(filter, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var log = await audit.GetAsync(id, ct);
        return log is null ? NotFound() : View(log);
    }
}
