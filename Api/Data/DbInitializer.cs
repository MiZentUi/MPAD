using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public static class DbInitializer
{
    public static async Task SeedData(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        if (await context.Services.AnyAsync())
            return;

        var baseUrl = app.Configuration["ApiBaseUrl"]!;

        var services = new List<Service>
        {
            new() { Name = "Cards", NormalizedName = "cards" },
            new() { Name = "Bonds", NormalizedName = "bonds" }
        };
        context.Services.AddRange(services);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new()
            {
                Name = "Debit card Black",
                Description = "The best debit card of 2026 according to the Compare award. Earn cashback and withdraw cash for free. Valid in 14 countries.",
                Image = $"{baseUrl}/images/tblack.png",
                Service = services[0]
            },
            new()
            {
                Name = "Islamic debit card",
                Description = "Complies with Islamic norms. Developed in collaboration with spiritual administrations and approved by muftis and ulema.",
                Image = $"{baseUrl}/images/thalal.png",
                Service = services[0]
            },
            new()
            {
                Name = "Ozon",
                Description = "Six-month return: −39.58%. Sector - Consumer goods and services",
                Image = $"{baseUrl}/images/ozon.png",
                Service = services[1]
            },
            new()
            {
                Name = "T-Technologies",
                Description = "Six-month return: −23.93%. Sector - Financial sector",
                Image = $"{baseUrl}/images/t-tech.png",
                Service = services[1]
            }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}
