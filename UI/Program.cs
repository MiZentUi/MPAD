using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using UI.Extensions;
using UI.HelperClasses;
using UI.Services.FileService;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IFileService, LocalFileService>();

builder.RegisterCustomServices();

var keycloakData = builder.Configuration
    .GetSection("Keycloak")
    .Get<KeycloakData>();

builder.Services
    .AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "keycloak";
        })
    .AddCookie()
    .AddOpenIdConnect("keycloak", options =>
        {
            options.Authority = $"{keycloakData!.Host}/auth/realms/{keycloakData.Realm}";
            options.ClientId = keycloakData.ClientId;
            options.ClientSecret = keycloakData.ClientSecret;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.Scope.Add("openid"); // Customize scopes as needed
            options.SaveTokens = true;
            options.RequireHttpsMetadata = false;
            options.MetadataAddress = $"{keycloakData.Host}/realms/{keycloakData.Realm}/.well-known/openid-configuration";
        });

builder.Services.AddAuthorization(opt => opt.AddPolicy("admin", p => p.RequireRole("POWER-USER")));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
    .RequireAuthorization("admin");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
