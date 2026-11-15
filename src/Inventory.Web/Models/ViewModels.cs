using Inventory.Application.Common;
using Inventory.Contracts;
using Inventory.Contracts.Enums;

namespace Inventory.Web.Models;

public sealed class CategoryFormModel
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UnitFormModel
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class ProductFormModel
{
    public Guid? Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid UnitId { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<SelectOption> Categories { get; set; } = Array.Empty<SelectOption>();
    public IReadOnlyList<SelectOption> Units { get; set; } = Array.Empty<SelectOption>();
}

public sealed class SupplierFormModel
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class LocationFormModel
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; } = LocationType.Branch;
    public Guid? ParentLocationId { get; set; }
    public string? BranchCode { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<SelectOption> Parents { get; set; } = Array.Empty<SelectOption>();
}

public sealed class UserFormModel
{
    public int? Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Password { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyList<UserOption> Parents { get; set; } = Array.Empty<UserOption>();
}

public sealed class ResetPasswordModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class LoginModel
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed class ChangePasswordModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class TransactionLineRow
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
}

public sealed class AdjustmentLineRow
{
    public Guid ProductId { get; set; }
    public int CountedQuantity { get; set; }
    public string? Notes { get; set; }
}

public sealed class OpnameLineRow
{
    public Guid DetailId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public int SystemQuantity { get; set; }
    public int? CountedQuantity { get; set; }
    public string? Notes { get; set; }
}

public sealed class TransactionFormModel
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string FormAction { get; set; } = "Create";
    public string Mode { get; set; } = "document";
    public bool IsEdit { get; set; }
    public bool ShowSupplier { get; set; }
    public bool ShowPurpose { get; set; }
    public bool ShowReason { get; set; }
    public bool ShowFromTo { get; set; }
    public bool ShowUnitCost { get; set; }
    public string? DocNo { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid LocationId { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public string? Purpose { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public List<TransactionLineRow> Lines { get; set; } = new();
    public List<AdjustmentLineRow> AdjustmentLines { get; set; } = new();
    public List<OpnameLineRow> OpnameLines { get; set; } = new();
    public IReadOnlyList<SelectOption> Products { get; set; } = Array.Empty<SelectOption>();
    public IReadOnlyList<SelectOption> Locations { get; set; } = Array.Empty<SelectOption>();
    public IReadOnlyList<SelectOption> Suppliers { get; set; } = Array.Empty<SelectOption>();
}

public sealed class TransactionIndexModel
{
    public string Title { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public PagedResult<TransactionListItem> Result { get; set; } = new();
    public TransactionFilter Filter { get; set; } = new();
    public bool ShowSecondaryLocation { get; set; }
    public bool ShowPartner { get; set; }
}

public sealed class TransactionDetailsModel
{
    public string Title { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string Mode { get; set; } = "document";
    public TransactionDetailView Detail { get; set; } = new();
}

public sealed class PagedReportModel<T>
{
    public string Title { get; set; } = string.Empty;
    public string ReportKey { get; set; } = string.Empty;
    public ReportFilter Filter { get; set; } = new();
    public PagedResult<T> Result { get; set; } = new();
}

public sealed class ReportsIndexModel
{
    public ReportFilter Filter { get; set; } = new();
    public DashboardSummary Summary { get; set; } = new();
}

public sealed class StockCardPageModel
{
    public ReportFilter Filter { get; set; } = new();
    public IReadOnlyList<StockCardItem> Items { get; set; } = Array.Empty<StockCardItem>();
}
