using Api.Abstractions;
using Api.Data;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features;

public static class AddNewProduct
{
    public sealed record RequestDTO(string Name, string Description, int ServiceId);

    public class RequestValidator : AbstractValidator<RequestDTO>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(r => r.Description)
                .NotEmpty();
            RuleFor(r => r.ServiceId)
                .GreaterThan(0);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Product", Handler)
                .DisableAntiforgery()
                .WithName("CreateProduct")
                .WithTags("Products");
        }
    }

    public static async Task<IResult> Handler(
        [FromBody] RequestDTO request,
        AppDbContext db,
        IValidator<RequestDTO> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return TypedResults.BadRequest(
                error: validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var serviceExists = db.Services.Any(s => s.Id == request.ServiceId);
        if (!serviceExists)
        {
            return TypedResults.BadRequest(error: "No such service");
        }

        var newProduct = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Service = db.Services.First(s => s.Id == request.ServiceId)
        };

        db.Products.Add(newProduct);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/api/Product/{newProduct.Id}", newProduct);
    }
}
