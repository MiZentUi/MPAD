using LR1_3.Models;
using Microsoft.AspNetCore.Mvc;

namespace LR1_3.Components;

public class CartViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View(new CartViewModel { Price = 9.99M, Count = 3 });
    }
}