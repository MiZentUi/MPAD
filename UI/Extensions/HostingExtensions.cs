using UI.Models;
using UI.Services.ProductService;
using UI.Services.ServiceService;

namespace UI.Extensions;

public static class HostingExtensions
{
    public static void RegisterCustomServices(this WebApplicationBuilder builder)
    {
        var uriData = builder.Configuration.GetSection("UriData").Get<UriData>()!;

        builder.Services.AddHttpClient<IServiceService, ApiServiceService>(
            opt => opt.BaseAddress = new Uri(uriData.ApiUri + "Service/"));

        builder.Services.AddHttpClient<IProductService, ApiProductService>(
            opt => opt.BaseAddress = new Uri(uriData.ApiUri + "Product/"));
    }
}
