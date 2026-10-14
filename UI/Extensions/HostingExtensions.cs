using UI.Controllers;
using UI.HelperClasses;
using UI.Models;
using UI.Services.Authentication;
using UI.Services.ProductService;
using UI.Services.ServiceService;

namespace UI.Extensions;

public static class HostingExtensions
{
    public static void RegisterCustomServices(this WebApplicationBuilder builder)
    {
        var uriData = builder.Configuration.GetSection("UriData").Get<UriData>()!;

        builder.Services.AddScoped<TokenHandler>();

        builder.Services.AddHttpClient<IServiceService, ApiServiceService>(
            opt => opt.BaseAddress = new Uri(uriData.ApiUri + "Service/"))
            .AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<IProductService, ApiProductService>(
            opt => opt.BaseAddress = new Uri(uriData.ApiUri + "Product/"))
            .AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<ITokenAccessor, KeycloakTokenAccessor>();

        builder.Services.AddHttpClient<AccountController>();

        builder.Services.Configure<KeycloakData>(builder.Configuration.GetSection("Keycloak"));
    }
}