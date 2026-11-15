using Inventory.Application.Common;
using Inventory.Application.Dtos;

namespace Inventory.Application.Abstractions;

public interface IUserService
{
    Task<PagedResult<UserListItem>> ListAsync(UserFilter filter, CancellationToken ct = default);
    Task<UserEditData?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UserOption>> GetParentCandidatesAsync(CancellationToken ct = default);
    Task<int> CreateAsync(UserInput input, CancellationToken ct = default);
    Task UpdateAsync(int id, UserInput input, CancellationToken ct = default);
    Task DeactivateAsync(int id, CancellationToken ct = default);
    Task ResetPasswordAsync(int id, string newPassword, CancellationToken ct = default);
}
