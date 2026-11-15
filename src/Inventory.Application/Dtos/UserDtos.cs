namespace Inventory.Application.Dtos;

public sealed class UserInput
{
    public int? Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public bool IsActive { get; init; } = true;
    public string? Password { get; init; }
}

public sealed class UserFilter
{
    public string? Search { get; set; }
    public string? RoleName { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class UserListItem
{
    public int Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public string? ParentName { get; init; }
    public bool IsActive { get; init; }
}

public sealed class UserOption
{
    public int Id { get; init; }
    public string Text { get; init; } = string.Empty;
}

public sealed class UserEditData
{
    public int Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public bool IsActive { get; init; }
}


