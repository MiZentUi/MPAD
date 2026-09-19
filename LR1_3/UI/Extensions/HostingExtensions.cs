using LR1_3.UI.Services.ProductService;

namespace LR1_3.UI.Extensions;

public static class HostingExtensions
{
    public static void RegisterCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IServiceService, MemoryServiceService>();
        builder.Services.AddScoped<IProductService, MemoryProductService>();
    }
}