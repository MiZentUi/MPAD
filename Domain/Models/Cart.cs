using Domain.Entities;

namespace Domain.Models;

/// <summary>
/// Корзина заказов (хранится в сессии)
/// </summary>
public class Cart
{
    /// <summary>
    /// Список объектов в корзине
    /// key - идентификатор объекта
    /// </summary>
    public Dictionary<int, CartItem> CartItems { get; set; } = new();

    /// <summary>
    /// Добавить объект в корзину.
    /// Если объект уже есть - увеличить количество.
    /// </summary>
    public virtual void AddToCart(Product product)
    {
        if (CartItems.TryGetValue(product.Id, out var item))
        {
            item.Count++;
        }
        else
        {
            CartItems[product.Id] = new CartItem { Item = product, Count = 1 };
        }
    }

    /// <summary>
    /// Удалить объект из корзины
    /// </summary>
    /// <param name="id">id удаляемого объекта</param>
    public virtual void RemoveItems(int id)
    {
        CartItems.Remove(id);
    }

    /// <summary>
    /// Очистить корзину
    /// </summary>
    public virtual void ClearAll()
    {
        CartItems.Clear();
    }

    /// <summary>
    /// Количество объектов в корзине
    /// </summary>
    public int Count => CartItems.Values.Sum(item => item.Count);
}
