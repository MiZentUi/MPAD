namespace Api.Services.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Интерфейс для доступа к токенам аутентификации Keycloak
/// </summary>
public interface ITokenAccessor
{
    /// <summary>
    /// Добавление заголовка Authorization: bearer
    /// </summary>
    /// <param name="httpClient">HttpClient, в который добавляется заголовок</param>
    /// <param name="isClient">если true - получить токен клиента; если false - получить токен пользователя</param>
    /// <returns>Task</returns>
    Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient);
}