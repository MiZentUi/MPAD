using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using UI.HelperClasses;

namespace UI.Services.Authentication;

/// <summary>
/// Реализация ITokenAccessor: токен клиента (client_credentials)
/// и токен текущего пользователя из сессии
/// </summary>
public class KeycloakTokenAccessor(
    IHttpContextAccessor contextAccessor,
    IOptions<KeycloakData> options,
    HttpClient httpClient) : ITokenAccessor
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
    private readonly KeycloakData _keycloak = options.Value;
    private readonly HttpClient _httpClient = httpClient;

    public async Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient)
    {
        string? token = isClient
            ? await GetClientTokenAsync()
            : await GetUserTokenAsync() ?? await GetClientTokenAsync();

        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
    }

    /// <summary>
    /// Токен пользователя из текущей сессии (куки-аутентификации).
    /// null, если пользователь не вошёл.
    /// </summary>
    private async Task<string?> GetUserTokenAsync()
    {
        var context = _contextAccessor.HttpContext;
        if (context?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var token = await context.GetTokenAsync("access_token");
        return string.IsNullOrEmpty(token) ? null : token;
    }

    /// <summary>
    /// Токен клиента по grant_type=client_credentials
    /// </summary>
    private async Task<string> GetClientTokenAsync()
    {
        var requestUri =
            $"{_keycloak.Host}/realms/{_keycloak.Realm}/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _keycloak.ClientId,
            ["client_secret"] = _keycloak.ClientSecret,
            ["grant_type"] = "client_credentials"
        });

        var response = await _httpClient.PostAsync(requestUri, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(response.StatusCode.ToString());
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("access_token").GetString()
            ?? throw new HttpRequestException("Не удалось получить access_token");
    }
}