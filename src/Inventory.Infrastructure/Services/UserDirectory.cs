using Inventory.Application.Abstractions;
using Inventory.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Services;

public sealed class UserDirectory(UserManager<AppUser> userManager) : IUserDirectory
{
    public Task<int?> GetParentIdAsync(int userId, CancellationToken ct = default) =>
        userManager.Users.Where(x => x.Id == userId).Select(x => x.ParentId).FirstOrDefaultAsync(ct);

    public Task<bool> IsActiveAsync(int userId, CancellationToken ct = default) =>
        userManager.Users.Where(x => x.Id == userId).Select(x => x.IsActive).FirstOrDefaultAsync(ct);

    public async Task<string?> GetDisplayNameAsync(int userId, CancellationToken ct = default) =>
        await userManager.Users.Where(x => x.Id == userId).Select(x => x.FullName).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<int, string>> GetDisplayNamesAsync(IEnumerable<int> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, string>();
        var users = await userManager.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.FullName })
            .ToListAsync(ct);
        return users.ToDictionary(x => x.Id, x => x.FullName);
    }
}
