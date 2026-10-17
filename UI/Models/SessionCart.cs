using System.Text.Json.Serialization;
using UI.Extensions;

namespace UI.Models;

/// <summary>
/// Корзина, хранящаяся в сессии. Изменения сразу сохраняются в сессию.
/// По: A. Freeman, Pro ASP.NET Core 6, ch. 9 «SportsStore: Completing the Cart».
/// </summary>
public class SessionCart : Cart
{
    public static Cart GetCart(IServiceProvider services)
    {
        var session = services.GetRequiredService<IHttpContextAccessor>().HttpContext?.Session;
        var cart = session?.Get<SessionCart>(SessionKey) ?? new SessionCart();
        cart.Session = session;
        return cart;
    }

    [JsonIgnore]
    public ISession? Session { get; set; }

    public override void AddToCart(Product product)
    {
        base.AddToCart(product);
        Session?.Set(SessionKey, this);
    }

    public override void RemoveItems(int id)
    {
        base.RemoveItems(id);
        Session?.Set(SessionKey, this);
    }

    public override void ClearAll()
    {
        base.ClearAll();
        Session?.Set(SessionKey, this);
    }

    private const string SessionKey = "cart";
}
