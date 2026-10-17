using Microsoft.AspNetCore.Mvc;
using UI.Extensions;
using UI.Services.ProductService;
using UI.Services.ServiceService;

namespace UI.Controllers;

public class ProductController(IProductService productService, IServiceService serviceService) : Controller
{
    [Route("Catalog")]
    [Route("Catalog/{service?}")]
    public async Task<IActionResult> Index(string? service, int pageNo = 1)
    {
        var productResponse = await productService.GetProductListAsync(service, pageNo);
        if (!productResponse.Successfull)
            return NotFound(productResponse.ErrorMessage);

        if (Request.IsAjaxRequest())
            return PartialView("_ListPartial", productResponse.Data);

        var services = await serviceService.GetServiceListAsync();
        ViewBag.Services = services.Data;
        ViewBag.CurrentService = null;
        ViewBag.CurrentPage = pageNo;

        return View(productResponse.Data);
    }
}
