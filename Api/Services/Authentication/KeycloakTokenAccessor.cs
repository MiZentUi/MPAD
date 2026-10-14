using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Api.Models;
using System.Net.Http.Headers;

namespace Api.Services.Authentication;

/// <summary>
/// Реализация интерфейса ITokenAccessor для получения токенов из Keycloak
/// </summary>
public class KeycloakTokenAccessor( IHttpContextAccessor contextAccessor,
    IOptions<KeycloakData> options,
    HttpClient httpClient) : ITokenAccessor
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
    private readonly KeycloakData _options = options.Value;

    public async Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient)
    {
        string token = isClient
            ? await GetClientToken()
            : await GetUserToken();

        httpClient
            .DefaultRequestHeaders
            .Authorization = new AuthenticationHeaderValue("bearer", token);
    }

    /// <summary>
    /// Получение токена пользователя из текущего HttpContext
    /// </summary>
    /// <returns>Токен доступа</returns>
    /// <exception cref="AuthenticationFailureException">Выбрасывается когда пользователь не авторизован</exception>
    async Task<string> GetUserToken()
    {
        var context = _contextAccessor.HttpContext;
        var authSession = await context.AuthenticateAsync("keycloak");
        if (authSession?.Principal == null)
        {
            throw new AuthenticationFailureException("Пользователь не авторизован");
        }
        return await context.GetTokenAsync("keycloak", "access_token");
    }

    /// <summary>
    /// Получение клиентского токента (client credentials grant type)
    /// </summary>
    /// <returns>Токен доступа клиента</returns>
    async Task<string> GetClientToken()
    {
        var requestUri =
            $"{_options.Host}/realms/{_options.Realm}/protocol/openid-connect/token";

        HttpContent content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>
            ("client_id", _options.ClientId),
            new KeyValuePair<string, string>
            ("grant_type", "client_credentials"),
            new KeyValuePair<string, string>
            ("client_secret", _options.ClientSecret)
        ]);

        var response = await httpClient.PostAsync(requestUri, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(response.StatusCode.ToString());
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonNode>(jsonString)?["access_token"]?.GetValue<string>()
            ?? throw new HttpRequestException("Failed to parse access token from response");
    }
}