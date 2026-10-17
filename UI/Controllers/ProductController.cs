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

        // get the list of services (categories)
        var servicesResponse = await serviceService.GetServiceListAsync();
        // if the list is not retrieved, return 404
        if (!servicesResponse.Successfull)
            return NotFound(servicesResponse.ErrorMessage);

        ViewBag.Services = servicesResponse.Data;
        ViewBag.CurrentService = service == null
            ? "All"
            : servicesResponse.Data?.FirstOrDefault(s => s.NormalizedName == service)?.Name;
        ViewBag.CurrentPage = pageNo;

        return View(productResponse.Data);
    }
}
