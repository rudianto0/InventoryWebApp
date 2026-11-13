using Inventory.Infrastructure.Identity;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[AllowAnonymous]
public sealed class AccountController(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByNameAsync(model.UserName.Trim());
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Username atau password salah.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
            ModelState.AddModelError(string.Empty, "Akun terkunci sementara karena terlalu banyak percobaan. Coba lagi dalam 5 menit.");
        else
            ModelState.AddModelError(string.Empty, "Username atau password salah.");

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        if (model.NewPassword != model.ConfirmPassword)
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Konfirmasi password tidak sama.");
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction(nameof(Login));

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Password berhasil diubah.";
        return RedirectToAction("Index", "Dashboard");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
