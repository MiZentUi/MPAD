using Api.Data;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.EndPoints;

public static class ServiceEndpoints
{
    public static void MapServiceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Service");

        group.MapGet("/",
            async (AppDbContext db) =>
            {
                var services = await db.Services
                    .AsNoTracking()
                    .ToListAsync();
                return TypedResults.Ok(
                    ResponseData<List<Service>>.Success(services));
            })
            .WithName("GetAllServices")
            .WithOpenApi();
    }
}
