using Inventory.Contracts;
using Inventory.Contracts.Enums;

namespace Inventory.Client;

public interface ICatalogClient
{
    Task<PagedResult<CategoryResponse>> GetCategoriesAsync(string? search = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default);
    Task<CategoryResponse?> GetCategoryAsync(Guid id, CancellationToken ct = default);
    Task<CategoryResponse> CreateCategoryAsync(CategoryInput input, CancellationToken ct = default);
    Task UpdateCategoryAsync(Guid id, CategoryInput input, CancellationToken ct = default);
    Task DeactivateCategoryAsync(Guid id, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<UnitResponse>> GetUnitsAsync(string? search = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default);
    Task<UnitResponse?> GetUnitAsync(Guid id, CancellationToken ct = default);
    Task<UnitResponse> CreateUnitAsync(UnitInput input, CancellationToken ct = default);
    Task UpdateUnitAsync(Guid id, UnitInput input, CancellationToken ct = default);
    Task DeleteUnitAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ProductListItem>> GetProductsAsync(string? search = null, Guid? categoryId = null, bool? activeOnly = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default);
    Task<ProductResponse?> GetProductAsync(Guid id, CancellationToken ct = default);
    Task<ProductResponse> CreateProductAsync(ProductInput input, CancellationToken ct = default);
    Task UpdateProductAsync(Guid id, ProductInput input, CancellationToken ct = default);
    Task DeactivateProductAsync(Guid id, CancellationToken ct = default);
    Task DeleteProductAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SelectOption>> GetCategoryOptionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SelectOption>> GetUnitOptionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SelectOption>> GetProductOptionsAsync(CancellationToken ct = default);
}

public interface ISupplierClient
{
    Task<PagedResult<SupplierResponse>> ListAsync(string? search = null, bool? activeOnly = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default);
    Task<SupplierResponse?> GetAsync(Guid id, CancellationToken ct = default);
    Task<SupplierResponse> CreateAsync(SupplierInput input, CancellationToken ct = default);
    Task UpdateAsync(Guid id, SupplierInput input, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SelectOption>> GetOptionsAsync(CancellationToken ct = default);
}

public interface ILocationClient
{
    Task<PagedResult<LocationResponse>> ListAsync(LocationType? type = null, bool? activeOnly = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default);
    Task<LocationResponse?> GetAsync(Guid id, CancellationToken ct = default);
    Task<LocationResponse> CreateAsync(LocationInput input, CancellationToken ct = default);
    Task UpdateAsync(Guid id, LocationInput input, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SelectOption>> GetOptionsAsync(bool activeOnly = true, CancellationToken ct = default);
}

public interface IStockInClient
{
    Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default);
    Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<TransactionCreatedResponse> CreateAsync(StockInInput input, CancellationToken ct = default);
    Task UpdateAsync(Guid id, StockInInput input, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SubmitAsync(Guid id, CancellationToken ct = default);
    Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default);
    Task RejectAsync(Guid id, string reason, CancellationToken ct = default);
}

public interface IStockOutClient
{
    Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default);
    Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<TransactionCreatedResponse> CreateAsync(StockOutInput input, CancellationToken ct = default);
    Task UpdateAsync(Guid id, StockOutInput input, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SubmitAsync(Guid id, CancellationToken ct = default);
    Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default);
    Task RejectAsync(Guid id, string reason, CancellationToken ct = default);
}

public interface IStockTransferClient
{
    Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default);
    Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<TransactionCreatedResponse> CreateAsync(StockTransferInput input, CancellationToken ct = default);
    Task UpdateAsync(Guid id, StockTransferInput input, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SubmitAsync(Guid id, CancellationToken ct = default);
    Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default);
    Task RejectAsync(Guid id, string reason, CancellationToken ct = default);
}

public interface IStockAdjustmentClient
{
    Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default);
    Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<TransactionCreatedResponse> CreateAsync(StockAdjustmentInput input, CancellationToken ct = default);
    Task UpdateAsync(Guid id, StockAdjustmentInput input, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SubmitAsync(Guid id, CancellationToken ct = default);
    Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default);
    Task RejectAsync(Guid id, string reason, CancellationToken ct = default);
}

public interface IStockOpnameClient
{
    Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default);
    Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<TransactionCreatedResponse> CreateAsync(StockOpnameInput input, CancellationToken ct = default);
    Task UpdateAsync(Guid id, IReadOnlyList<OpnameLineInput> lines, string? notes, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SubmitAsync(Guid id, CancellationToken ct = default);
    Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default);
    Task RejectAsync(Guid id, string reason, CancellationToken ct = default);
}

public interface IApprovalClient
{
    Task<IReadOnlyList<ApprovalItem>> GetPendingAsync(SortSpec? sort = null, CancellationToken ct = default);
}

public interface IReportClient
{
    Task<DashboardSummary> GetDashboardAsync(Guid? locationId = null, CancellationToken ct = default);
    Task<PagedResult<StockBalanceItem>> GetStockBalanceAsync(ReportFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<StockCardItem>> GetStockCardAsync(ReportFilter filter, CancellationToken ct = default);
    Task<PagedResult<StockMovementItem>> GetMovementsAsync(ReportFilter filter, CancellationToken ct = default);
    Task<PagedResult<ReorderItem>> GetReorderAsync(ReportFilter filter, CancellationToken ct = default);
    Task<string> ExportCsvAsync(string report, ReportFilter filter, CancellationToken ct = default);
}

public interface IAuditClient
{
    Task<PagedResult<AuditListItem>> ListAsync(AuditFilter filter, CancellationToken ct = default);
    Task<AuditDetailItem?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetEntityNamesAsync(CancellationToken ct = default);
}

public interface IUserClient
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
