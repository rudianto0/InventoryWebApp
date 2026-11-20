using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Inventory.Contracts;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public class CatalogCategoryTests
{
    [Fact]
    public async Task CreateCategory_TrimsAndPersists()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);

        var category = await service.CreateCategoryAsync(new CategoryInput { Name = "  Supplies  ", Description = "  desc  " });

        category.Name.Should().Be("Supplies");
        category.Description.Should().Be("desc");
        category.CreatedById.Should().Be(1);
    }

    [Fact]
    public async Task CreateCategory_RequiresName()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);

        var action = () => service.CreateCategoryAsync(new CategoryInput { Name = "   " });

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateCategory_RejectsDuplicateName()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        await service.CreateCategoryAsync(new CategoryInput { Name = "General" });

        var action = () => service.CreateCategoryAsync(new CategoryInput { Name = "General" });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*sudah digunakan*");
    }

    [Fact]
    public async Task UpdateCategory_RejectsDuplicateName()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        var first = await service.CreateCategoryAsync(new CategoryInput { Name = "First" });
        await service.CreateCategoryAsync(new CategoryInput { Name = "Second" });

        var action = () => service.UpdateCategoryAsync(first.Id, new CategoryInput { Name = "Second" });

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateCategory_ThrowsWhenMissing()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);

        var action = () => service.UpdateCategoryAsync(Guid.NewGuid(), new CategoryInput { Name = "X" });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*tidak ditemukan*");
    }

    [Fact]
    public async Task DeactivateCategory_RejectsWhenActiveProductsExist()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);

        var action = () => service.DeactivateCategoryAsync(fixture.CategoryId);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*produk aktif*");
    }

    [Fact]
    public async Task DeactivateCategory_SucceedsWhenUnused()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        var category = await service.CreateCategoryAsync(new CategoryInput { Name = "Unused" });

        await service.DeactivateCategoryAsync(category.Id);

        (await fixture.Db.Categories.SingleAsync(x => x.Id == category.Id)).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCategory_RejectsWhenUsedByProduct()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);

        var action = () => service.DeleteCategoryAsync(fixture.CategoryId);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*tidak dapat dihapus*");
    }

    [Fact]
    public async Task DeleteCategory_SoftDeletesWhenUnused()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        var category = await service.CreateCategoryAsync(new CategoryInput { Name = "Unused" });

        await service.DeleteCategoryAsync(category.Id);

        (await fixture.Db.Categories.AnyAsync(x => x.Id == category.Id)).Should().BeFalse();
        (await fixture.Db.Categories.IgnoreQueryFilters().SingleAsync(x => x.Id == category.Id)).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetCategories_FiltersBySearchAndOrdersByName()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        await service.CreateCategoryAsync(new CategoryInput { Name = "Beta" });
        await service.CreateCategoryAsync(new CategoryInput { Name = "Alpha" });

        var result = await service.GetCategoriesAsync("a");

        result.Items.Select(x => x.Name).Should().Contain(new[] { "Alpha", "Beta" });
        result.Items.Select(x => x.Name).Should().BeInAscendingOrder();
    }
}

public class CatalogUnitTests
{
    [Fact]
    public async Task CreateUnit_NormalizesCodeAndTrimsName()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);

        var unit = await service.CreateUnitAsync(new UnitInput { Code = " box ", Name = " Boxes " });

        unit.Code.Should().Be("BOX");
        unit.Name.Should().Be("Boxes");
    }

    [Theory]
    [InlineData("", "Name")]
    [InlineData("PCS", "")]
    public async Task CreateUnit_RequiresCodeAndName(string code, string name)
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);

        var action = () => service.CreateUnitAsync(new UnitInput { Code = code, Name = name });

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateUnit_RejectsDuplicateCode()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        await service.CreateUnitAsync(new UnitInput { Code = "PCS", Name = "Pieces" });

        var action = () => service.CreateUnitAsync(new UnitInput { Code = "pcs", Name = "Pieces 2" });

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*sudah digunakan*");
    }

    [Fact]
    public async Task UpdateUnit_RejectsDuplicateCode()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        var first = await service.CreateUnitAsync(new UnitInput { Code = "PCS", Name = "Pieces" });
        await service.CreateUnitAsync(new UnitInput { Code = "BOX", Name = "Boxes" });

        var action = () => service.UpdateUnitAsync(first.Id, new UnitInput { Code = "BOX", Name = "Pieces" });

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteUnit_RejectsWhenUsedByProduct()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);

        var action = () => service.DeleteUnitAsync(fixture.UnitId);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*masih dipakai produk*");
    }

    [Fact]
    public async Task DeleteUnit_SoftDeletesWhenUnused()
    {
        await using var fixture = new TestDatabase();
        var service = new CatalogService(fixture.Db);
        var unit = await service.CreateUnitAsync(new UnitInput { Code = "KG", Name = "Kilogram" });

        await service.DeleteUnitAsync(unit.Id);

        (await fixture.Db.Units.AnyAsync(x => x.Id == unit.Id)).Should().BeFalse();
    }
}

public class CatalogProductTests
{
    private static ProductInput ValidProduct(TestDatabase fixture, string sku = "SKU-NEW") => new()
    {
        Sku = sku,
        Name = "New Product",
        CategoryId = fixture.CategoryId,
        UnitId = fixture.UnitId,
        ReorderLevel = 5
    };

    [Fact]
    public async Task CreateProduct_NormalizesSku()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);

        var product = await service.CreateProductAsync(ValidProduct(fixture, " sku-new "));

        product.Sku.Should().Be("SKU-NEW");
        product.CreatedAt.Should().NotBe(default);
    }

    [Theory]
    [InlineData("", "Name")]
    [InlineData("SKU", "")]
    public async Task CreateProduct_RequiresSkuAndName(string sku, string name)
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);
        var input = ValidProduct(fixture, sku);
        input = new ProductInput { Sku = sku, Name = name, CategoryId = input.CategoryId, UnitId = input.UnitId };

        var action = () => service.CreateProductAsync(input);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProduct_RejectsNegativeReorderLevel()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);
        var input = new ProductInput { Sku = "SKU-X", Name = "X", CategoryId = fixture.CategoryId, UnitId = fixture.UnitId, ReorderLevel = -1 };

        var action = () => service.CreateProductAsync(input);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Reorder level*");
    }

    [Fact]
    public async Task CreateProduct_RejectsSkuWithSpace()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);

        var action = () => service.CreateProductAsync(ValidProduct(fixture, "SKU X"));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*spasi*");
    }

    [Fact]
    public async Task CreateProduct_RejectsDuplicateSku()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);

        var action = () => service.CreateProductAsync(ValidProduct(fixture, "sku-1"));

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*SKU sudah digunakan*");
    }

    [Fact]
    public async Task CreateProduct_RejectsMissingOrInactiveCategory()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);
        var missing = new ProductInput { Sku = "SKU-M", Name = "M", CategoryId = Guid.NewGuid(), UnitId = fixture.UnitId };
        var inactiveCategory = new Category { Name = "Inactive", IsActive = false };
        fixture.Db.Categories.Add(inactiveCategory);
        await fixture.Db.SaveChangesAsync();
        var inactive = new ProductInput { Sku = "SKU-I", Name = "I", CategoryId = inactiveCategory.Id, UnitId = fixture.UnitId };

        await FluentActions.Awaiting(() => service.CreateProductAsync(missing))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Kategori tidak ditemukan*");
        await FluentActions.Awaiting(() => service.CreateProductAsync(inactive))
            .Should().ThrowAsync<ValidationException>().WithMessage("*Kategori tidak aktif*");
    }

    [Fact]
    public async Task CreateProduct_RejectsMissingUnit()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);
        var input = new ProductInput { Sku = "SKU-X", Name = "X", CategoryId = fixture.CategoryId, UnitId = Guid.NewGuid() };

        var action = () => service.CreateProductAsync(input);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*Unit tidak ditemukan*");
    }

    [Fact]
    public async Task UpdateProduct_AllowsKeepingOwnSku()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);

        await service.UpdateProductAsync(fixture.ProductId, ValidProduct(fixture, "SKU-1"));

        (await fixture.Db.Products.SingleAsync(x => x.Id == fixture.ProductId)).Name.Should().Be("New Product");
    }

    [Fact]
    public async Task DeleteProduct_RejectsWhenStockHistoryExists()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedStockAsync(fixture.ProductId, fixture.WarehouseId, 1);
        var service = new CatalogService(fixture.Db);

        var action = () => service.DeleteProductAsync(fixture.ProductId);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("*riwayat stok*");
    }

    [Fact]
    public async Task DeleteProduct_SoftDeletesWhenUnused()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);
        var product = await fixture.SeedProductAsync("SKU-UNUSED");

        await service.DeleteProductAsync(product.Id);

        (await fixture.Db.Products.AnyAsync(x => x.Id == product.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task GetProducts_AppliesFilters()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedProductAsync("SKU-INACTIVE", isActive: false);
        var service = new CatalogService(fixture.Db);

        var active = await service.GetProductsAsync(activeOnly: true);
        var filtered = await service.GetProductsAsync("INACTIVE", categoryId: fixture.CategoryId);

        active.Items.Should().OnlyContain(x => x.IsActive);
        filtered.Items.Should().ContainSingle(x => x.Sku == "SKU-INACTIVE");
    }

    [Fact]
    public async Task ProductOptions_ReturnOnlyActive()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        await fixture.SeedProductAsync("SKU-INACTIVE", isActive: false);
        var service = new CatalogService(fixture.Db);

        var options = await service.GetProductOptionsAsync();

        options.Should().ContainSingle();
        options.Single().Text.Should().StartWith("SKU-1");
    }

    [Fact]
    public async Task DeactivateProduct_SetsInactive()
    {
        await using var fixture = new TestDatabase();
        await fixture.SeedBasicsAsync();
        var service = new CatalogService(fixture.Db);

        await service.DeactivateProductAsync(fixture.ProductId);

        (await fixture.Db.Products.SingleAsync(x => x.Id == fixture.ProductId)).IsActive.Should().BeFalse();
    }
}