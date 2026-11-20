using Inventory.Client;
using Inventory.Contracts;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class UsersController(IUserClient users) : AppControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? roleName, bool? isActive, string? sortBy, SortDirection sortDir, int page = 1, CancellationToken ct = default)
    {
        var filter = new UserFilter { Search = search, RoleName = roleName, IsActive = isActive, SortBy = sortBy, SortDir = sortDir, Page = page };
        ViewBag.Roles = await users.GetRolesAsync(ct);
        ViewBag.Search = search;
        ViewBag.RoleName = roleName;
        ViewBag.IsActive = isActive;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDir = sortDir;
        return View(await users.ListAsync(filter, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct) =>
        await BuildFormAsync(new UserFormModel(), ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormModel model, CancellationToken ct)
    {
        try
        {
            await users.CreateAsync(Map(model), ct);
            NotifySuccess("User berhasil dibuat.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            return await BuildFormAsync(model, ct);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var data = await users.GetAsync(id, ct);
        if (data is null)
            return NotFound();
        return await BuildFormAsync(new UserFormModel
        {
            Id = data.Id,
            UserName = data.UserName,
            Email = data.Email,
            FullName = data.FullName,
            RoleName = data.RoleName,
            ParentId = data.ParentId,
            IsActive = data.IsActive
        }, ct);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormModel model, CancellationToken ct)
    {
        try
        {
            await users.UpdateAsync(id, Map(model), ct);
            NotifySuccess("User berhasil diperbarui.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            return await BuildFormAsync(model, ct);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        try
        {
            await users.DeactivateAsync(id, ct);
            NotifySuccess("User dinonaktifkan.");
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            NotifyError(FormError(ex)!);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken ct)
    {
        var data = await users.GetAsync(id, ct);
        if (data is null)
            return NotFound();
        return View(new ResetPasswordModel { UserId = data.Id, UserName = data.UserName });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model, CancellationToken ct)
    {
        try
        {
            await users.ResetPasswordAsync(model.UserId, model.NewPassword, ct);
            NotifySuccess("Password user berhasil direset.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (FormError(ex) is not null)
        {
            ModelState.AddModelError(string.Empty, FormError(ex)!);
            return View(model);
        }
    }

    private async Task<IActionResult> BuildFormAsync(UserFormModel model, CancellationToken ct)
    {
        model.Roles = await users.GetRolesAsync(ct);
        model.Parents = await users.GetParentCandidatesAsync(ct);
        return View("Form", model);
    }

    private static UserInput Map(UserFormModel model) => new()
    {
        Id = model.Id,
        UserName = model.UserName,
        Email = model.Email,
        FullName = model.FullName,
        RoleName = model.RoleName,
        ParentId = model.ParentId,
        IsActive = model.IsActive,
        Password = model.Password
    };
}
