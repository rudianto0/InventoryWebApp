using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryClients(this IServiceCollection services)
    {
        services.AddScoped<ICatalogClient, InProcessCatalogClient>();
        services.AddScoped<ISupplierClient, InProcessSupplierClient>();
        services.AddScoped<ILocationClient, InProcessLocationClient>();
        services.AddScoped<IStockInClient, InProcessStockInClient>();
        services.AddScoped<IStockOutClient, InProcessStockOutClient>();
        services.AddScoped<IStockTransferClient, InProcessStockTransferClient>();
        services.AddScoped<IStockAdjustmentClient, InProcessStockAdjustmentClient>();
        services.AddScoped<IStockOpnameClient, InProcessStockOpnameClient>();
        services.AddScoped<IApprovalClient, InProcessApprovalClient>();
        services.AddScoped<IReportClient, InProcessReportClient>();
        services.AddScoped<IAuditClient, InProcessAuditClient>();
        services.AddScoped<IUserClient, InProcessUserClient>();
        return services;
    }
}
