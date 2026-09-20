using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UI.Services.ServiceService;

namespace UI.Services.ProductService;

public class MemoryProductService : IProductService
{
    List<Product> _products;
    List<Service> _services;
    readonly int _perPage;

    public MemoryProductService([FromServices] IConfiguration config, IServiceService serviceService)
    {
        _perPage = config.GetValue<int>("ItemsPerPage");
        _services = serviceService.GetServiceListAsync().Result.Data!;
        SetupData();
    }

    public Task DeleteProductAsync(int id)
    {
        throw new NotImplementedException();
    }

    Task<ResponseData<Product>> IProductService.CreateProductAsync(Product product, IFormFile? formFile)
    {
        throw new NotImplementedException();
    }

    Task<ResponseData<Product>> IProductService.GetProductByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    async Task<ResponseData<ListModel<Product>>> IProductService.GetProductListAsync(string? serviceNormalizedName, int pageNo)
    {
        var data = _products
            .Where(p => serviceNormalizedName == null || p.Service!.NormalizedName!.Equals(serviceNormalizedName))
            .Skip(Math.Min(_products.Count, _perPage * Math.Max(0, pageNo - 1)))
            .Take(_perPage)
            .ToList();
        return ResponseData<ListModel<Product>>.Success(new ListModel<Product>
        {
            Items = data
        });
    }

    /// <summary>
    /// Инициализация списков
    /// </summary>
    private void SetupData()
    {
        _products = [
            new() {Id = 1, Name="Debit card Black",
                Description="The best debit card of 2026 according to the Compare award. Earn cashback and withdraw cash for free. Valid in 14 countries.",
                Image="../images/tblack.png", Service = _services.Find(s=>s.NormalizedName!.Equals("cards"))},
            new() {Id = 2, Name="Islamic debit card",
                Description="Complies with Islamic norms. Developed in collaboration with spiritual administrations and approved by muftis and ulema.",
                Image="../images/thalal.png", Service = _services.Find(s=>s.NormalizedName!.Equals("cards"))},
            new() {Id = 3, Name="Ozon", Description="Six-month return: −39.58%. Sector - Consumer goods and services",
                Image="../images/ozon.png", Service = _services.Find(s=>s.NormalizedName!.Equals("bonds"))},
            new() {Id = 4, Name="T-Technologies", Description="Six-month return: −23.93%. Sector - Financial sector",
                Image="../images/t-tech.png", Service = _services.Find(s=>s.NormalizedName!.Equals("bonds"))},
        ];
    }

    Task IProductService.UpdateProductAsync(int id, Product product, IFormFile? formFile)
    {
        throw new NotImplementedException();
    }
}