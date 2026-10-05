using Api.Abstractions;
using Api.Data;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features;

public static class GetProductById
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/product/{id:int}", Handler)
                .DisableAntiforgery()
                .WithName("GetProductById")
                .WithTags("Products")
                .AllowAnonymous();
        }
    }

    public static async Task<IResult> Handler(int id, AppDbContext db)
    {
        var product = await db.Products
            .Include(p => p.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(ResponseData<Product>.Success(product));
    }
}
