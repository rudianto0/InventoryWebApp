using System.Security.Claims;
using Inventory.Application.Abstractions;

namespace Inventory.Web.Services;

public sealed class HttpCurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public int? UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => accessor.HttpContext?.User.Identity?.Name;

    public bool IsInRole(string role) => accessor.HttpContext?.User.IsInRole(role) ?? false;
}
