using Api.Abstractions;
using Api.Data;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features;

public static class GetAllProducts
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/product/{serviceNormalizedName:alpha?}", Handler)
                .DisableAntiforgery()
                .WithName("GetAllProducts")
                .WithTags("Products");
        }
    }

    public static async Task<IResult> Handler(
        string? serviceNormalizedName,
        AppDbContext db,
        int pageNo = 1,
        int pageSize = 3)
    {
        var maxPageSize = 20;

        var query = db.Products
            .Include(p => p.Service)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(serviceNormalizedName))
        {
            query = query.Where(p =>
                p.Service != null &&
                p.Service.NormalizedName == serviceNormalizedName);
        }

        var totalCount = await query.CountAsync();

        var size = Math.Min(pageSize, maxPageSize);
        var page = Math.Max(1, pageNo);
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);

        var items = await query
            .Skip(size * (page - 1))
            .Take(size)
            .ToListAsync();

        return TypedResults.Ok(
            ResponseData<ListModel<Product>>.Success(new ListModel<Product>
            {
                Items = items,
                CurrentPage = page,
                TotalPages = totalPages
            }));
    }
}
