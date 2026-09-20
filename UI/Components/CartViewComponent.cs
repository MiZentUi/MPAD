using UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace UI.Components;

public class CartViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View(new CartViewModel { Price = 9.99M, Count = 3 });
    }
}