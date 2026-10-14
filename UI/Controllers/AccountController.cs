using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UI.HelperClasses;
using UI.Models;
using UI.Services.Authentication;
using UI.Services.FileService;

namespace UI.Controllers;

public class AccountController(
    HttpClient httpClient,
    ITokenAccessor tokenAccessor,
    IOptions<KeycloakData> options,
    IFileService fileService) : Controller
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ITokenAccessor _tokenAccessor = tokenAccessor;
    private readonly KeycloakData _keycloak = options.Value;
    private readonly IFileService _fileService = fileService;

    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    #region Register

    /// <summary>
    /// Форма регистрации нового пользователя
    /// </summary>
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterUserViewModel());
    }

    /// <summary>
    /// Регистрация пользователя на сервере Keycloak (Admin API)
    /// </summary>
    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Register(RegisterUserViewModel user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        if (user is null)
        {
            return BadRequest();
        }

        try
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient, true);
        }
        catch (Exception)
        {
            return Unauthorized();
        }

        var avatarUrl = "/images/default-profile-picture.png";
        if (user.Avatar is not null)
        {
            avatarUrl = await _fileService.SaveFileAsync(user.Avatar);
        }

        var newUser = new CreateUserModel
        {
            Username = user.Email,
            Email = user.Email,
            Enabled = true,
            EmailVerified = true,
            Attributes = new Dictionary<string, string> { ["avatar"] = avatarUrl },
            Credentials = new List<UserCredentials>
            {
                new() { Type = "password", Temporary = false, Value = user.Password }
            }
        };

        var requestUri =
            $"{_keycloak.Host}/admin/realms/{_keycloak.Realm}/users";
        var userData = JsonSerializer.Serialize(newUser, SerializerOptions);
        HttpContent content = new StringContent(userData, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(requestUri, content);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Login));
        }

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty,
            $"Ошибка регистрации: {response.StatusCode}. {error}");
        return View(user);
    }

    #endregion

    #region Login

    /// <summary>
    /// Отдельная страница входа (форма)
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    /// <summary>
    /// Вход: проверка учётных данных на сервере Keycloak
    /// (grant_type=password) и создание локальной сессии
    /// </summary>
    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var requestUri =
            $"{_keycloak.Host}/realms/{_keycloak.Realm}/protocol/openid-connect/token";
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _keycloak.ClientId,
            ["client_secret"] = _keycloak.ClientSecret,
            ["username"] = model.Email,
            ["password"] = model.Password
        });

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(requestUri, form);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Сервер аутентификации недоступен");
            return View(model);
        }

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль");
            return View(model);
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;

        var storedTokens = new List<AuthenticationToken>
        {
            new() { Name = "access_token", Value = accessToken }
        };
        if (doc.RootElement.TryGetProperty("id_token", out var idToken))
        {
            storedTokens.Add(new AuthenticationToken { Name = "id_token", Value = idToken.GetString()! });
        }

        var principal = CreatePrincipal(accessToken);
        var properties = new AuthenticationProperties();
        properties.StoreTokens(storedTokens);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        return Redirect(Url.Action("Index", "Home"));
    }

    #endregion

    #region Logout

    /// <summary>
    /// Выход из системы (уничтожение куки-сессии)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(Url.Action("Index", "Home"));
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Формирование ClaimsPrincipal из access-token (JWT)
    /// </summary>
    private ClaimsPrincipal CreatePrincipal(string accessToken)
    {
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

        try
        {
            var payload = accessToken.Split('.')[1]
                .Replace('-', '+')
                .Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));

            using var doc = JsonDocument.Parse(json);
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "realm_access":
                        if (property.Value.TryGetProperty("roles", out var realmRoles))
                        {
                            foreach (var role in realmRoles.EnumerateArray())
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                            }
                        }
                        break;

                    case "resource_access":
                        if (property.Value.TryGetProperty(_keycloak.ClientId, out var client) &&
                            client.TryGetProperty("roles", out var clientRoles))
                        {
                            foreach (var role in clientRoles.EnumerateArray())
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                            }
                        }
                        break;

                    default:
                        if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            var value = property.Value.GetString()!;
                            identity.AddClaim(new Claim(property.Name, value));

                            if (property.Name.Equals("preferred_username",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Name, value));
                            }
                        }
                        else if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in property.Value.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String)
                                {
                                    identity.AddClaim(new Claim(property.Name, item.GetString()!));
                                }
                            }
                        }
                        break;
                }
            }
        }
        catch (Exception)
        {
        }

        return new ClaimsPrincipal(identity);
    }

    #endregion
}

/// <summary>
/// Данные о пользователе для Admin API Keycloak (UserRepresentation)
/// </summary>
internal class CreateUserModel
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; } = true;
    public Dictionary<string, string> Attributes { get; set; } = new();
    public List<UserCredentials> Credentials { get; set; } = new();
}

/// <summary>
/// Учётные данные пользователя (пароль)
/// </summary>
internal class UserCredentials
{
    public string Type { get; set; } = "password";
    public bool Temporary { get; set; }
    public string Value { get; set; } = string.Empty;
}