using System.Text.Json;

namespace UI.Extensions;

/// <summary>
/// Расширяющие методы для сохранения произвольных объектов в сессии (JSON)
/// </summary>
public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value is null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
