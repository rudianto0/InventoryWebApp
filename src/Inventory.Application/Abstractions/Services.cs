namespace Inventory.Application.Abstractions;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsInRole(string role);
}

public interface IUserDirectory
{
    Task<int?> GetParentIdAsync(int userId, CancellationToken ct = default);
    Task<bool> IsActiveAsync(int userId, CancellationToken ct = default);
    Task<string?> GetDisplayNameAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<int, string>> GetDisplayNamesAsync(IEnumerable<int> userIds, CancellationToken ct = default);
}

public interface IDocumentNumberService
{
    Task<string> NextAsync(string prefix, DateTime date, CancellationToken ct = default);
}
