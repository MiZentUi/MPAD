using LR1_2.Models;
using Microsoft.AspNetCore.Mvc;

namespace LR1_2.Components;

public class CartViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View(new CartViewModel { Price = 9.99M, Count = 3 });
    }
}