using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.Use_Cases;

namespace Api.EndPoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Product");

        group.MapGet("/{serviceNormalizedName:alpha?}",
            async (IMediator mediator, string? serviceNormalizedName,
                int pageNo = 1, int pageSize = 3) =>
            {
                var data = await mediator.Send(
                    new GetListOfProducts(serviceNormalizedName, pageNo, pageSize));
                return TypedResults.Ok(data);
            })
            .WithName("GetAllProducts")
            .WithOpenApi();
    }
}
