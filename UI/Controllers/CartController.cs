using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Services.ProductService;

namespace UI.Controllers;

[Authorize]
public class CartController(IProductService productService, Cart cart) : Controller
{
    private readonly IProductService _productService = productService;
    private readonly Cart _cart = cart;

    public IActionResult Index()
    {
        return View(_cart);
    }

    [Route("[controller]/add/{id:int}")]
    public async Task<ActionResult> Add(int id, string? returnUrl)
    {
        // asp-route-return-url формирует query-ключ "return-url",
        // забираем его как fallback
        returnUrl ??= Request.Query["return-url"].ToString();
        if (string.IsNullOrEmpty(returnUrl))
            returnUrl = Url.Action("Index", "Product") ?? "/Catalog";

        var data = await _productService.GetProductByIdAsync(id);
        if (data.Successfull && data.Data is not null)
        {
            _cart.AddToCart(data.Data);
        }

        return Redirect(returnUrl);
    }

    [Route("[controller]/remove/{id:int}")]
    public IActionResult Remove(int id)
    {
        _cart.RemoveItems(id);
        return RedirectToAction(nameof(Index));
    }

    [Route("[controller]/clear")]
    public IActionResult Clear()
    {
        _cart.ClearAll();
        return RedirectToAction(nameof(Index));
    }
}
