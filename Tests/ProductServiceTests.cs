using Api.Data;
using Api.Use_Cases;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Tests;

/// <summary>
/// Tests for the GetProductListAsync method of the ProductService (task 2).
/// GetListOfProductsHandler plays the role of ProductService in this project.
/// </summary>
public class ProductServiceTests
{
    /// <summary>
    /// Creates a database context using the in-memory provider
    /// </summary>
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    /// <summary>
    /// Adds a service and the specified number of products linked to it to the context
    /// </summary>
    private static List<Product> SeedProducts(AppDbContext context, int count,
        string normalizedName = "cards")
    {
        var service = new Service
        {
            Name = normalizedName,
            NormalizedName = normalizedName
        };
        context.Services.Add(service);

        var idOffset = context.Products.Count();
        var products = Enumerable.Range(1, count)
            .Select(i => new Product
            {
                Id = idOffset + i,
                Name = $"Product {idOffset + i}",
                Service = service
            })
            .ToList();

        context.Products.AddRange(products);
        context.SaveChanges();
        return products;
    }

    [Fact]
    public async Task ServiceReturnsFirstPageOfThreeItems()
    {
        using var context = CreateContext();
        var products = SeedProducts(context, 4);
        var handler = new GetListOfProductsHandler(context);

        var result = await handler.Handle(new GetListOfProducts(null), CancellationToken.None);

        Assert.IsType<ResponseData<ListModel<Product>>>(result);
        Assert.True(result.Successfull);
        var data = result.Data!;
        Assert.Equal(1, data.CurrentPage);
        Assert.Equal(3, data.Items.Count);
        Assert.Equal(2, data.TotalPages);
        Assert.Equal(products[0].Id, data.Items[0].Id);
    }

    [Fact]
    public async Task ServiceReturnsRequestedPage()
    {
        using var context = CreateContext();
        var products = SeedProducts(context, 7);
        var handler = new GetListOfProductsHandler(context);

        var result = await handler.Handle(
            new GetListOfProducts(null, pageNo: 2), CancellationToken.None);

        Assert.True(result.Successfull);
        var data = result.Data!;
        Assert.Equal(2, data.CurrentPage);
        Assert.Equal(3, data.TotalPages);
        Assert.Equal(3, data.Items.Count);
        Assert.Equal(products[3].Id, data.Items[0].Id);
        Assert.Equal(products[5].Id, data.Items[2].Id);
    }

    [Fact]
    public async Task ServiceFiltersProductsByServiceNormalizedName()
    {
        using var context = CreateContext();
        SeedProducts(context, 4, "cards");
        SeedProducts(context, 2, "bonds");
        var handler = new GetListOfProductsHandler(context);

        var result = await handler.Handle(
            new GetListOfProducts("cards"), CancellationToken.None);

        Assert.True(result.Successfull);
        var data = result.Data!;
        Assert.Equal(3, data.Items.Count);
        Assert.Equal(2, data.TotalPages);
        Assert.All(data.Items,
            product => Assert.Equal("cards", product.Service!.NormalizedName));
    }

    [Fact]
    public async Task ServiceLimitsPageSizeToMaximum()
    {
        using var context = CreateContext();
        SeedProducts(context, 25);
        var handler = new GetListOfProductsHandler(context);

        // the maximum page size is 20 items
        var result = await handler.Handle(
            new GetListOfProducts(null, pageNo: 1, pageSize: 100), CancellationToken.None);

        Assert.True(result.Successfull);
        var data = result.Data!;
        Assert.Equal(20, data.Items.Count);
        Assert.Equal(2, data.TotalPages);
    }

    [Fact]
    public async Task ServiceReturnsErrorWhenPageExceedsTotalPages()
    {
        using var context = CreateContext();
        SeedProducts(context, 4); // 4 items in total, i.e. 2 pages
        var handler = new GetListOfProductsHandler(context);

        var result = await handler.Handle(
            new GetListOfProducts(null, pageNo: 5), CancellationToken.None);

        Assert.False(result.Successfull);
        Assert.NotNull(result.ErrorMessage);
        Assert.Null(result.Data);
    }
}
