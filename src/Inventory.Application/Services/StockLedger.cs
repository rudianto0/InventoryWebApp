using System.ComponentModel.DataAnnotations;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Inventory.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Services;

public sealed record StockChange(
    MovementType Type,
    ReferenceType ReferenceType,
    Guid ReferenceId,
    Guid ProductId,
    Guid LocationId,
    int Quantity,
    int UserId,
    string? Notes);

public sealed class StockLedger(IAppDbContext db, IDocumentNumberService documents)
{
    public async Task ApplyAsync(IEnumerable<StockChange> changes, CancellationToken ct = default)
    {
        foreach (var change in changes)
        {
            if (change.Quantity == 0)
                continue;

            var stock = await db.Stocks.FirstOrDefaultAsync(
                x => x.ProductId == change.ProductId && x.LocationId == change.LocationId, ct)
                ?? db.Stocks.Local.FirstOrDefault(
                    x => x.ProductId == change.ProductId && x.LocationId == change.LocationId);

            if (stock is null)
            {
                stock = new Stock { ProductId = change.ProductId, LocationId = change.LocationId, Quantity = 0 };
                db.Stocks.Add(stock);
            }

            var newQuantity = stock.Quantity + change.Quantity;
            if (newQuantity < 0)
            {
                var product = await db.Products.Where(x => x.Id == change.ProductId).Select(x => x.Name).FirstOrDefaultAsync(ct);
                var location = await db.Locations.Where(x => x.Id == change.LocationId).Select(x => x.Name).FirstOrDefaultAsync(ct);
                throw new ValidationException($"Stok tidak mencukupi untuk produk '{product}' di lokasi '{location}'.");
            }

            stock.Quantity = newQuantity;
            stock.UpdatedAt = DateTime.UtcNow;
            stock.Version++;

            db.StockMovements.Add(new StockMovement
            {
                MovementNo = await documents.NextAsync("MOV", DateTime.UtcNow, ct),
                Type = change.Type,
                ProductId = change.ProductId,
                LocationId = change.LocationId,
                Quantity = change.Quantity,
                ReferenceType = change.ReferenceType,
                ReferenceId = change.ReferenceId,
                Notes = change.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedById = change.UserId
            });
        }
    }
}
