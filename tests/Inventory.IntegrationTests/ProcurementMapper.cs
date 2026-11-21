using Inventory.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Inventory.IntegrationTests;

internal sealed record ProcurementOrder(
    string PoNumber,
    string SupplierCode,
    string LocationCode,
    IReadOnlyList<ProcurementLine> Lines);

internal sealed record ProcurementLine(string Sku, int Quantity, decimal? UnitCost);

internal static class ProcurementMapper
{
    public static async Task<StockInInput> ToStockInInputAsync(
        IntegrationDatabase fixture,
        ProcurementOrder order,
        CancellationToken ct = default)
    {
        var supplier = await fixture.Db.Suppliers.SingleAsync(x => x.Code == order.SupplierCode, ct);
        var location = await fixture.Db.Locations.SingleAsync(x => x.Code == order.LocationCode, ct);
        var skus = order.Lines.Select(x => x.Sku).ToList();
        var products = await fixture.Db.Products
            .Where(x => skus.Contains(x.Sku))
            .ToDictionaryAsync(x => x.Sku, ct);

        return new StockInInput
        {
            SupplierId = supplier.Id,
            LocationId = location.Id,
            Date = new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc),
            Notes = $"Procurement {order.PoNumber}",
            Lines = order.Lines.Select(line => new DocumentLineInput
            {
                ProductId = products[line.Sku].Id,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                Notes = order.PoNumber
            }).ToList()
        };
    }
}
