using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Web.Controllers;

[Authorize]
public abstract class AppControllerBase : Controller
{
    protected void NotifySuccess(string message) => TempData["Success"] = message;
    protected void NotifyError(string message) => TempData["Error"] = message;

    protected string? FormError(Exception ex)
    {
        if (ex is ValidationException or InvalidOperationException)
            return ex.Message;
        if (ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            return "Data telah berubah oleh pengguna lain. Muat ulang halaman dan coba lagi.";
        return null;
    }
}
