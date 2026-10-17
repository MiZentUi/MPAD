using UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace UI.Components;

public class CartViewComponent(Cart cart) : ViewComponent
{
    private readonly Cart _cart = cart;

    public IViewComponentResult Invoke()
    {
        return View(new CartViewModel { Price = 0M, Count = _cart.Count });
    }
}
