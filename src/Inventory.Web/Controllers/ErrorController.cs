using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[AllowAnonymous]
public sealed class ErrorController : Controller
{
    public IActionResult Index() => View("~/Views/Shared/Error.cshtml");

    public IActionResult StatusCodePage(int code)
    {
        ViewBag.StatusCode = code;
        return View("~/Views/Shared/Error.cshtml");
    }
}
