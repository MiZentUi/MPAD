using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using UI.Controllers;
using UI.Services.ProductService;
using UI.Services.ServiceService;

namespace Tests;

/// <summary>
/// Tests for the Index method of the Product controller (task 1).
/// Services play the role of categories in this project.
/// </summary>
public class ProductControllerTests
{
    private readonly IProductService _productService = Substitute.For<IProductService>();
    private readonly IServiceService _serviceService = Substitute.For<IServiceService>();
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        // HttpContext is required so the controller can access the Request property
        _controller = new ProductController(_productService, _serviceService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static ListModel<Product> CreateProductsModel()
    {
        return new ListModel<Product>
        {
            Items =
            [
                new Product { Id = 1, Name = "Product 1" },
                new Product { Id = 2, Name = "Product 2" },
                new Product { Id = 3, Name = "Product 3" }
            ],
            CurrentPage = 1,
            TotalPages = 1
        };
    }

    private static List<Service> CreateServices()
    {
        return
        [
            new Service { Id = 1, Name = "Cards", NormalizedName = "cards" },
            new Service { Id = 2, Name = "Bonds", NormalizedName = "bonds" }
        ];
    }

    [Fact]
    public async Task IndexReturnsNotFoundWhenProductListFails()
    {
        _productService
            .GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>())
            .Returns(ResponseData<ListModel<Product>>.Error("Failed to get product list"));

        var result = await _controller.Index(null);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Failed to get product list", notFound.Value);
    }

    [Fact]
    public async Task IndexReturnsNotFoundWhenServiceListFails()
    {
        _productService
            .GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>())
            .Returns(ResponseData<ListModel<Product>>.Success(CreateProductsModel()));
        _serviceService
            .GetServiceListAsync()
            .Returns(ResponseData<List<Service>>.Error("Failed to get service list"));

        var result = await _controller.Index(null);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Failed to get service list", notFound.Value);
    }

    [Fact]
    public async Task IndexPassesServicesToViewData()
    {
        var services = CreateServices();
        _productService
            .GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>())
            .Returns(ResponseData<ListModel<Product>>.Success(CreateProductsModel()));
        _serviceService
            .GetServiceListAsync()
            .Returns(ResponseData<List<Service>>.Success(services));

        var result = await _controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(services, viewResult.ViewData["Services"]);
    }

    [Fact]
    public async Task IndexSetsCurrentServiceToAllWhenServiceIsNull()
    {
        _productService
            .GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>())
            .Returns(ResponseData<ListModel<Product>>.Success(CreateProductsModel()));
        _serviceService
            .GetServiceListAsync()
            .Returns(ResponseData<List<Service>>.Success(CreateServices()));

        var result = await _controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("All", viewResult.ViewData["CurrentService"]);
    }

    [Fact]
    public async Task IndexSetsCurrentServiceNameWhenServiceIsSpecified()
    {
        _productService
            .GetProductListAsync("bonds", 1)
            .Returns(ResponseData<ListModel<Product>>.Success(CreateProductsModel()));
        _serviceService
            .GetServiceListAsync()
            .Returns(ResponseData<List<Service>>.Success(CreateServices()));

        var result = await _controller.Index("bonds");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Bonds", viewResult.ViewData["CurrentService"]);
    }

    [Fact]
    public async Task IndexPassesProductListModelToView()
    {
        var model = CreateProductsModel();
        _productService
            .GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>())
            .Returns(ResponseData<ListModel<Product>>.Success(model));
        _serviceService
            .GetServiceListAsync()
            .Returns(ResponseData<List<Service>>.Success(CreateServices()));

        var result = await _controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public async Task IndexReturnsPartialViewForAjaxRequest()
    {
        var model = CreateProductsModel();
        _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _productService
            .GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>())
            .Returns(ResponseData<ListModel<Product>>.Success(model));

        var result = await _controller.Index(null);

        var partialView = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_ListPartial", partialView.ViewName);
        Assert.Same(model, partialView.Model);
    }
}
