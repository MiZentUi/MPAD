using System.Text.Json;
using Api.Abstractions;
using Api.Data;
using Api.Use_Cases;
using Domain.Entities;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features;

public static class AddNewProduct
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/product", Handler)
                .DisableAntiforgery()
                .WithName("CreateProduct")
                .WithTags("Products")
                .RequireAuthorization("admin");
        }
    }

    public static async Task<IResult> Handler(
        [FromForm] string product,
        [FromForm] IFormFile? file,
        AppDbContext db,
        IMediator mediator)
    {
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

        var newProduct = new Product
        {
            Name = request.Name,
            Description = request.Description
        };

        if (request.Service is not null)
        {
            var service = await db.Services.FindAsync(request.Service.Id);
            if (service is null)
            {
                return TypedResults.BadRequest(error: "No such service");
            }
            newProduct.Service = service;
        }

        if (file is not null)
        {
            newProduct.Image = await mediator.Send(new SaveImage(file));
        }

        db.Products.Add(newProduct);
        await db.SaveChangesAsync();

        return TypedResults.Created(
            $"/api/product/{newProduct.Id}",
            ResponseData<Product>.Success(newProduct));
    }
}
