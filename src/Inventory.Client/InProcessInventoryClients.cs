using Inventory.Application.Abstractions;
using Inventory.Application.Services;
using Inventory.Contracts;
using Inventory.Contracts.Enums;

namespace Inventory.Client;

public sealed class InProcessCatalogClient(CatalogService service) : ICatalogClient
{
    public Task<PagedResult<CategoryResponse>> GetCategoriesAsync(string? search = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default) => service.GetCategoriesAsync(search, page, pageSize, sort, ct);
    public Task<CategoryResponse?> GetCategoryAsync(Guid id, CancellationToken ct = default) => service.GetCategoryAsync(id, ct);
    public Task<CategoryResponse> CreateCategoryAsync(CategoryInput input, CancellationToken ct = default) => service.CreateCategoryAsync(input, ct);
    public Task UpdateCategoryAsync(Guid id, CategoryInput input, CancellationToken ct = default) => service.UpdateCategoryAsync(id, input, ct);
    public Task DeactivateCategoryAsync(Guid id, CancellationToken ct = default) => service.DeactivateCategoryAsync(id, ct);
    public Task DeleteCategoryAsync(Guid id, CancellationToken ct = default) => service.DeleteCategoryAsync(id, ct);
    public Task<PagedResult<UnitResponse>> GetUnitsAsync(string? search = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default) => service.GetUnitsAsync(search, page, pageSize, sort, ct);
    public Task<UnitResponse?> GetUnitAsync(Guid id, CancellationToken ct = default) => service.GetUnitAsync(id, ct);
    public Task<UnitResponse> CreateUnitAsync(UnitInput input, CancellationToken ct = default) => service.CreateUnitAsync(input, ct);
    public Task UpdateUnitAsync(Guid id, UnitInput input, CancellationToken ct = default) => service.UpdateUnitAsync(id, input, ct);
    public Task DeleteUnitAsync(Guid id, CancellationToken ct = default) => service.DeleteUnitAsync(id, ct);
    public Task<PagedResult<ProductListItem>> GetProductsAsync(string? search = null, Guid? categoryId = null, bool? activeOnly = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default) => service.GetProductsAsync(search, categoryId, activeOnly, page, pageSize, sort, ct);
    public Task<ProductResponse?> GetProductAsync(Guid id, CancellationToken ct = default) => service.GetProductAsync(id, ct);
    public Task<ProductResponse> CreateProductAsync(ProductInput input, CancellationToken ct = default) => service.CreateProductAsync(input, ct);
    public Task UpdateProductAsync(Guid id, ProductInput input, CancellationToken ct = default) => service.UpdateProductAsync(id, input, ct);
    public Task DeactivateProductAsync(Guid id, CancellationToken ct = default) => service.DeactivateProductAsync(id, ct);
    public Task DeleteProductAsync(Guid id, CancellationToken ct = default) => service.DeleteProductAsync(id, ct);
    public Task<IReadOnlyList<SelectOption>> GetCategoryOptionsAsync(CancellationToken ct = default) => service.GetCategoryOptionsAsync(ct);
    public Task<IReadOnlyList<SelectOption>> GetUnitOptionsAsync(CancellationToken ct = default) => service.GetUnitOptionsAsync(ct);
    public Task<IReadOnlyList<SelectOption>> GetProductOptionsAsync(CancellationToken ct = default) => service.GetProductOptionsAsync(ct);
}

public sealed class InProcessSupplierClient(SupplierService service) : ISupplierClient
{
    public Task<PagedResult<SupplierResponse>> ListAsync(string? search = null, bool? activeOnly = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default) => service.ListAsync(search, activeOnly, page, pageSize, sort, ct);
    public Task<SupplierResponse?> GetAsync(Guid id, CancellationToken ct = default) => service.GetAsync(id, ct);
    public Task<SupplierResponse> CreateAsync(SupplierInput input, CancellationToken ct = default) => service.CreateAsync(input, ct);
    public Task UpdateAsync(Guid id, SupplierInput input, CancellationToken ct = default) => service.UpdateAsync(id, input, ct);
    public Task DeactivateAsync(Guid id, CancellationToken ct = default) => service.DeactivateAsync(id, ct);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => service.DeleteAsync(id, ct);
    public Task<IReadOnlyList<SelectOption>> GetOptionsAsync(CancellationToken ct = default) => service.GetOptionsAsync(ct);
}

public sealed class InProcessLocationClient(LocationService service) : ILocationClient
{
    public Task<PagedResult<LocationResponse>> ListAsync(LocationType? type = null, bool? activeOnly = null, int page = 1, int pageSize = 20, SortSpec? sort = null, CancellationToken ct = default) => service.ListAsync(type, activeOnly, page, pageSize, sort, ct);
    public Task<LocationResponse?> GetAsync(Guid id, CancellationToken ct = default) => service.GetAsync(id, ct);
    public Task<LocationResponse> CreateAsync(LocationInput input, CancellationToken ct = default) => service.CreateAsync(input, ct);
    public Task UpdateAsync(Guid id, LocationInput input, CancellationToken ct = default) => service.UpdateAsync(id, input, ct);
    public Task DeactivateAsync(Guid id, CancellationToken ct = default) => service.DeactivateAsync(id, ct);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => service.DeleteAsync(id, ct);
    public Task<IReadOnlyList<SelectOption>> GetOptionsAsync(bool activeOnly = true, CancellationToken ct = default) => service.GetOptionsAsync(activeOnly, ct);
}

public sealed class InProcessStockInClient(StockInService service) : IStockInClient
{
    public Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default) => service.ListAsync(filter, ct);
    public Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default) => service.GetDetailAsync(id, ct);
    public Task<TransactionCreatedResponse> CreateAsync(StockInInput input, CancellationToken ct = default) => service.CreateAsync(input, ct);
    public Task UpdateAsync(Guid id, StockInInput input, CancellationToken ct = default) => service.UpdateAsync(id, input, ct);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => service.DeleteAsync(id, ct);
    public Task SubmitAsync(Guid id, CancellationToken ct = default) => service.SubmitAsync(id, ct);
    public Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default) => service.ApproveAsync(id, notes, ct);
    public Task RejectAsync(Guid id, string reason, CancellationToken ct = default) => service.RejectAsync(id, reason, ct);
}

public sealed class InProcessStockOutClient(StockOutService service) : IStockOutClient
{
    public Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default) => service.ListAsync(filter, ct);
    public Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default) => service.GetDetailAsync(id, ct);
    public Task<TransactionCreatedResponse> CreateAsync(StockOutInput input, CancellationToken ct = default) => service.CreateAsync(input, ct);
    public Task UpdateAsync(Guid id, StockOutInput input, CancellationToken ct = default) => service.UpdateAsync(id, input, ct);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => service.DeleteAsync(id, ct);
    public Task SubmitAsync(Guid id, CancellationToken ct = default) => service.SubmitAsync(id, ct);
    public Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default) => service.ApproveAsync(id, notes, ct);
    public Task RejectAsync(Guid id, string reason, CancellationToken ct = default) => service.RejectAsync(id, reason, ct);
}

public sealed class InProcessStockTransferClient(StockTransferService service) : IStockTransferClient
{
    public Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default) => service.ListAsync(filter, ct);
    public Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default) => service.GetDetailAsync(id, ct);
    public Task<TransactionCreatedResponse> CreateAsync(StockTransferInput input, CancellationToken ct = default) => service.CreateAsync(input, ct);
    public Task UpdateAsync(Guid id, StockTransferInput input, CancellationToken ct = default) => service.UpdateAsync(id, input, ct);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => service.DeleteAsync(id, ct);
    public Task SubmitAsync(Guid id, CancellationToken ct = default) => service.SubmitAsync(id, ct);
    public Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default) => service.ApproveAsync(id, notes, ct);
    public Task RejectAsync(Guid id, string reason, CancellationToken ct = default) => service.RejectAsync(id, reason, ct);
}

public sealed class InProcessStockAdjustmentClient(StockAdjustmentService service) : IStockAdjustmentClient
{
    public Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default) => service.ListAsync(filter, ct);
    public Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default) => service.GetDetailAsync(id, ct);
    public Task<TransactionCreatedResponse> CreateAsync(StockAdjustmentInput input, CancellationToken ct = default) => service.CreateAsync(input, ct);
    public Task UpdateAsync(Guid id, StockAdjustmentInput input, CancellationToken ct = default) => service.UpdateAsync(id, input, ct);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => service.DeleteAsync(id, ct);
    public Task SubmitAsync(Guid id, CancellationToken ct = default) => service.SubmitAsync(id, ct);
    public Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default) => service.ApproveAsync(id, notes, ct);
    public Task RejectAsync(Guid id, string reason, CancellationToken ct = default) => service.RejectAsync(id, reason, ct);
}

public sealed class InProcessStockOpnameClient(StockOpnameService service) : IStockOpnameClient
{
    public Task<PagedResult<TransactionListItem>> ListAsync(TransactionFilter filter, CancellationToken ct = default) => service.ListAsync(filter, ct);
    public Task<TransactionDetailView?> GetDetailAsync(Guid id, CancellationToken ct = default) => service.GetDetailAsync(id, ct);
    public Task<TransactionCreatedResponse> CreateAsync(StockOpnameInput input, CancellationToken ct = default) => service.CreateAsync(input, ct);
    public Task UpdateAsync(Guid id, IReadOnlyList<OpnameLineInput> lines, string? notes, CancellationToken ct = default) => service.UpdateAsync(id, lines, notes, ct);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => service.DeleteAsync(id, ct);
    public Task SubmitAsync(Guid id, CancellationToken ct = default) => service.SubmitAsync(id, ct);
    public Task ApproveAsync(Guid id, string? notes = null, CancellationToken ct = default) => service.ApproveAsync(id, notes, ct);
    public Task RejectAsync(Guid id, string reason, CancellationToken ct = default) => service.RejectAsync(id, reason, ct);
}

public sealed class InProcessApprovalClient(ApprovalService service) : IApprovalClient
{
    public Task<IReadOnlyList<ApprovalItem>> GetPendingAsync(SortSpec? sort = null, CancellationToken ct = default) => service.GetPendingAsync(sort, ct);
}

public sealed class InProcessReportClient(ReportService service) : IReportClient
{
    public Task<DashboardSummary> GetDashboardAsync(Guid? locationId = null, CancellationToken ct = default) => service.GetDashboardAsync(locationId, ct);
    public Task<PagedResult<StockBalanceItem>> GetStockBalanceAsync(ReportFilter filter, CancellationToken ct = default) => service.GetStockBalanceAsync(filter, ct);
    public Task<IReadOnlyList<StockCardItem>> GetStockCardAsync(ReportFilter filter, CancellationToken ct = default) => service.GetStockCardAsync(filter, ct);
    public Task<PagedResult<StockMovementItem>> GetMovementsAsync(ReportFilter filter, CancellationToken ct = default) => service.GetMovementsAsync(filter, ct);
    public Task<PagedResult<ReorderItem>> GetReorderAsync(ReportFilter filter, CancellationToken ct = default) => service.GetReorderAsync(filter, ct);
    public Task<string> ExportCsvAsync(string report, ReportFilter filter, CancellationToken ct = default) => service.ExportCsvAsync(report, filter, ct);
}

public sealed class InProcessAuditClient(AuditQueryService service) : IAuditClient
{
    public Task<PagedResult<AuditListItem>> ListAsync(AuditFilter filter, CancellationToken ct = default) => service.ListAsync(filter, ct);
    public Task<AuditDetailItem?> GetAsync(int id, CancellationToken ct = default) => service.GetAsync(id, ct);
    public Task<IReadOnlyList<string>> GetEntityNamesAsync(CancellationToken ct = default) => service.GetEntityNamesAsync(ct);
}

public sealed class InProcessUserClient(IUserService service) : IUserClient
{
    public Task<PagedResult<UserListItem>> ListAsync(UserFilter filter, CancellationToken ct = default) => service.ListAsync(filter, ct);
    public Task<UserEditData?> GetAsync(int id, CancellationToken ct = default) => service.GetAsync(id, ct);
    public Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken ct = default) => service.GetRolesAsync(ct);
    public Task<IReadOnlyList<UserOption>> GetParentCandidatesAsync(CancellationToken ct = default) => service.GetParentCandidatesAsync(ct);
    public Task<int> CreateAsync(UserInput input, CancellationToken ct = default) => service.CreateAsync(input, ct);
    public Task UpdateAsync(int id, UserInput input, CancellationToken ct = default) => service.UpdateAsync(id, input, ct);
    public Task DeactivateAsync(int id, CancellationToken ct = default) => service.DeactivateAsync(id, ct);
    public Task ResetPasswordAsync(int id, string newPassword, CancellationToken ct = default) => service.ResetPasswordAsync(id, newPassword, ct);
}
