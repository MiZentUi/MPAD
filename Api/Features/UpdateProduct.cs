using System.Text.Json;
using Api.Abstractions;
using Api.Data;
using Api.Use_Cases;
using Domain.Entities;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features;

public static class UpdateProduct
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("api/product/{id:int}", Handler)
                .DisableAntiforgery()
                .WithName("UpdateProduct")
                .WithTags("Products")
                .RequireAuthorization("admin");
        }
    }

    public static async Task<IResult> Handler(
        int id,
        [FromForm] string product,
        [FromForm] IFormFile? file,
        AppDbContext db,
        IMediator mediator)
    {
        var existing = await db.Products
            .Include(p => p.Service)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existing is null)
        {
            return TypedResults.NotFound();
        }

        var request = JsonSerializer.Deserialize<Product>(product, JsonOptions);
        if (request is null)
        {
            return TypedResults.BadRequest(error: "Invalid product data");
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Description))
        {
            return TypedResults.BadRequest(error: "Name and description are required");
        }

        existing.Name = request.Name;
        existing.Description = request.Description;

        if (request.Service is not null)
        {
            var service = await db.Services.FindAsync(request.Service.Id);
            if (service is null)
            {
                return TypedResults.BadRequest(error: "No such service");
            }
            existing.Service = service;
        }

        if (file is not null)
        {
            var oldImage = existing.Image;
            existing.Image = await mediator.Send(new SaveImage(file));
            if (!string.IsNullOrEmpty(oldImage))
            {
                await mediator.Send(new DeleteImage(oldImage));
            }
        }

        await db.SaveChangesAsync();

        return TypedResults.Ok(ResponseData<Product>.Success(existing));
    }
}
