namespace UI.Services.Authentication;

/// <summary>
/// Сервис получения токенов аутентификации Keycloak
/// </summary>
public interface ITokenAccessor
{
    /// <summary>
    /// Добавление заголовка Authorization: bearer
    /// </summary>
    /// <param name="httpClient">HttpClient, в который добавляется заголовок</param>
    /// <param name="isClient">если true - получить токен клиента;
    /// если false - получить токен пользователя</param>
    Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient);
}