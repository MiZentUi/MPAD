using Microsoft.AspNetCore.Mvc;

namespace LR1_3.Controllers;

public class ProductController(IProductService productService, IServiceService serviceService) : Controller
{
    public async Task<IActionResult> Index(string? service, int pageNo = 1)
    {
        var services = await serviceService.GetServiceListAsync();
        ViewBag.Services = services.Data;
        ViewBag.CurrentService = null;
        ViewBag.CurrentPage = pageNo;

        var productResponse = await productService.GetProductListAsync(service, pageNo);
        if (!productResponse.Successfull)
            return NotFound(productResponse.ErrorMessage);
        return View(productResponse.Data);
    }
}