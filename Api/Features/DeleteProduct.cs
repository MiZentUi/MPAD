using Api.Abstractions;
using Api.Data;
using Api.Use_Cases;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features;

public static class DeleteProduct
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/product/{id:int}", Handler)
                .DisableAntiforgery()
                .WithName("DeleteProduct")
                .WithTags("Products")
                .RequireAuthorization("admin");
        }
    }

    public static async Task<IResult> Handler(int id, AppDbContext db, IMediator mediator)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return TypedResults.NotFound();
        }

        if (!string.IsNullOrEmpty(product.Image))
        {
            await mediator.Send(new DeleteImage(product.Image));
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
