using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Services;

public sealed class UserService(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    ICurrentUserService currentUser) : IUserService
{
    public async Task<PagedResult<UserListItem>> ListAsync(UserFilter filter, CancellationToken ct = default)
    {
        var users = await userManager.Users.AsNoTracking().OrderBy(x => x.UserName).ToListAsync(ct);
        var parents = users.ToDictionary(x => x.Id, x => x.FullName);

        var roleLookup = new Dictionary<int, string>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            roleLookup[user.Id] = roles.FirstOrDefault() ?? string.Empty;
        }

        IEnumerable<AppUser> filtered = users;
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            filtered = filtered.Where(x =>
                x.UserName!.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Email!.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.FullName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(filter.RoleName))
            filtered = filtered.Where(x => roleLookup.GetValueOrDefault(x.Id) == filter.RoleName);
        if (filter.IsActive.HasValue)
            filtered = filtered.Where(x => x.IsActive == filter.IsActive.Value);

        var sortKey = Sorting.NormalizeKey(filter.SortBy);
        var desc = Sorting.IsDesc(filter.SortDir);
        IOrderedEnumerable<AppUser> ordered = (sortKey, desc) switch
        {
            ("username", true) => filtered.OrderByDescending(x => x.UserName!.ToLowerInvariant()),
            ("fullname", false) => filtered.OrderBy(x => x.FullName.ToLowerInvariant()),
            ("fullname", true) => filtered.OrderByDescending(x => x.FullName.ToLowerInvariant()),
            ("email", false) => filtered.OrderBy(x => x.Email!.ToLowerInvariant()),
            ("email", true) => filtered.OrderByDescending(x => x.Email!.ToLowerInvariant()),
            ("role", false) => filtered.OrderBy(x => (roleLookup.GetValueOrDefault(x.Id) ?? string.Empty).ToLowerInvariant()),
            ("role", true) => filtered.OrderByDescending(x => (roleLookup.GetValueOrDefault(x.Id) ?? string.Empty).ToLowerInvariant()),
            ("parent", false) => filtered.OrderBy(x => !x.ParentId.HasValue).ThenBy(x => x.ParentId.HasValue ? (parents.GetValueOrDefault(x.ParentId.Value) ?? string.Empty).ToLowerInvariant() : string.Empty),
            ("parent", true) => filtered.OrderBy(x => !x.ParentId.HasValue).ThenByDescending(x => x.ParentId.HasValue ? (parents.GetValueOrDefault(x.ParentId.Value) ?? string.Empty).ToLowerInvariant() : string.Empty),
            ("status", false) => filtered.OrderBy(x => x.IsActive),
            ("status", true) => filtered.OrderByDescending(x => x.IsActive),
            _ => filtered.OrderBy(x => x.UserName!.ToLowerInvariant())
        };
        var list = ordered.ThenBy(x => x.Id).ToList();
        var total = list.Count;
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        return new PagedResult<UserListItem>
        {
            Items = list.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new UserListItem
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                Email = x.Email ?? string.Empty,
                FullName = x.FullName,
                RoleName = roleLookup.GetValueOrDefault(x.Id) ?? string.Empty,
                ParentName = x.ParentId.HasValue ? parents.GetValueOrDefault(x.ParentId.Value) : null,
                IsActive = x.IsActive
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserEditData?> GetAsync(int id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return null;
        var roles = await userManager.GetRolesAsync(user);
        return new UserEditData
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            RoleName = roles.FirstOrDefault() ?? string.Empty,
            ParentId = user.ParentId,
            IsActive = user.IsActive
        };
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken ct = default) =>
        await roleManager.Roles.AsNoTracking().OrderBy(x => x.Name).Select(x => x.Name!).ToListAsync(ct);

    public async Task<IReadOnlyList<UserOption>> GetParentCandidatesAsync(CancellationToken ct = default)
    {
        var result = new List<UserOption>();
        foreach (var role in new[] { "SPV", "Manager" })
        {
            var users = await userManager.GetUsersInRoleAsync(role);
            result.AddRange(users.Where(x => x.IsActive).Select(x => new UserOption
            {
                Id = x.Id,
                Text = $"{x.FullName} ({x.UserName})"
            }));
        }
        return result.OrderBy(x => x.Text).ToList();
    }

    public async Task<int> CreateAsync(UserInput input, CancellationToken ct = default)
    {
        Guard.Required(input.UserName, "Username");
        Guard.Required(input.Email, "Email");
        Guard.Required(input.FullName, "Nama lengkap");
        Guard.Required(input.RoleName, "Role");
        Guard.Required(input.Password, "Password");
        if (input.Password!.Length < 8)
            throw new ValidationException("Password minimal 8 karakter.");
        if (!await roleManager.RoleExistsAsync(input.RoleName))
            throw new ValidationException("Role tidak valid.");
        if (await userManager.FindByNameAsync(input.UserName.Trim()) is not null)
            throw new ValidationException("Username sudah digunakan.");
        if (await userManager.FindByEmailAsync(input.Email.Trim()) is not null)
            throw new ValidationException("Email sudah digunakan.");
        await ValidateParentAsync(input.ParentId, null, ct);

        var user = new AppUser
        {
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim(),
            FullName = input.FullName.Trim(),
            ParentId = input.ParentId,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, input.Password);
        EnsureSuccess(result);
        await userManager.AddToRoleAsync(user, input.RoleName);
        return user.Id;
    }

    public async Task UpdateAsync(int id, UserInput input, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new ValidationException("User tidak ditemukan.");
        Guard.Required(input.Email, "Email");
        Guard.Required(input.FullName, "Nama lengkap");
        Guard.Required(input.RoleName, "Role");
        if (!await roleManager.RoleExistsAsync(input.RoleName))
            throw new ValidationException("Role tidak valid.");
        await ValidateParentAsync(input.ParentId, id, ct);

        var emailOwner = await userManager.FindByEmailAsync(input.Email.Trim());
        if (emailOwner is not null && emailOwner.Id != id)
            throw new ValidationException("Email sudah digunakan.");

        var roles = await userManager.GetRolesAsync(user);
        var currentRole = roles.FirstOrDefault() ?? string.Empty;
        if (currentRole == "Admin" && (input.RoleName != "Admin" || !input.IsActive))
            await EnsureAnotherActiveAdminAsync(id);

        if (!string.Equals(user.UserName, input.UserName, StringComparison.Ordinal))
        {
            Guard.Required(input.UserName, "Username");
            var nameOwner = await userManager.FindByNameAsync(input.UserName.Trim());
            if (nameOwner is not null && nameOwner.Id != id)
                throw new ValidationException("Username sudah digunakan.");
            var nameResult = await userManager.SetUserNameAsync(user, input.UserName.Trim());
            EnsureSuccess(nameResult);
        }

        var emailResult = await userManager.SetEmailAsync(user, input.Email.Trim());
        EnsureSuccess(emailResult);
        user.FullName = input.FullName.Trim();
        user.ParentId = input.ParentId;
        user.IsActive = input.IsActive;
        EnsureSuccess(await userManager.UpdateAsync(user));

        if (!roles.Contains(input.RoleName))
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, roles);
            EnsureSuccess(removeResult);
            EnsureSuccess(await userManager.AddToRoleAsync(user, input.RoleName));
        }
    }

    public async Task DeactivateAsync(int id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new ValidationException("User tidak ditemukan.");
        if (currentUser.UserId == id)
            throw new ValidationException("Anda tidak dapat menonaktifkan akun sendiri.");

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
            await EnsureAnotherActiveAdminAsync(id);

        user.IsActive = false;
        EnsureSuccess(await userManager.UpdateAsync(user));
    }

    public async Task ResetPasswordAsync(int id, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            throw new ValidationException("Password minimal 8 karakter.");
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new ValidationException("User tidak ditemukan.");
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        EnsureSuccess(await userManager.ResetPasswordAsync(user, token, newPassword));
    }

    private async Task ValidateParentAsync(int? parentId, int? currentId, CancellationToken ct)
    {
        if (!parentId.HasValue)
            return;
        if (currentId.HasValue && parentId.Value == currentId.Value)
            throw new ValidationException("Parent tidak boleh diri sendiri.");
        var parent = await userManager.FindByIdAsync(parentId.Value.ToString())
            ?? throw new ValidationException("Parent tidak ditemukan.");
        if (!parent.IsActive)
            throw new ValidationException("Parent tidak aktif.");
        var roles = await userManager.GetRolesAsync(parent);
        if (!roles.Any(r => r is "SPV" or "Manager"))
            throw new ValidationException("Parent harus memiliki role SPV atau Manager.");
    }

    private async Task EnsureAnotherActiveAdminAsync(int excludingUserId)
    {
        var admins = await userManager.GetUsersInRoleAsync("Admin");
        if (admins.Count(x => x.IsActive && x.Id != excludingUserId) == 0)
            throw new ValidationException("Admin aktif terakhir tidak boleh dinonaktifkan atau diubah rolenya.");
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (result.Succeeded)
            return;
        var message = string.Join(" ", result.Errors.Select(x => x.Description));
        throw new ValidationException(string.IsNullOrWhiteSpace(message) ? "Operasi user gagal." : message);
    }
}
