using Domain.Entities;

namespace Domain.Models;

/// <summary>
/// Один элемент корзины заказов
/// </summary>
public class CartItem
{
    public Product? Item { get; set; }
    public int Count { get; set; }
}
