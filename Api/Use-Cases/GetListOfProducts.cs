using Api.Data;
using Domain.Entities;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Use_Cases;

public sealed record GetListOfProducts(
    string? serviceNormalizedName,
    int pageNo = 1,
    int pageSize = 3) : IRequest<ResponseData<ListModel<Product>>>;

public class GetListOfProductsHandler(AppDbContext db) :
    IRequestHandler<GetListOfProducts, ResponseData<ListModel<Product>>>
{
    private readonly int _maxPageSize = 20;

    public async Task<ResponseData<ListModel<Product>>> Handle(
        GetListOfProducts request, CancellationToken cancellationToken)
    {
        var query = db.Products
            .Include(p => p.Service)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(request.serviceNormalizedName))
        {
            query = query.Where(p =>
                p.Service != null &&
                p.Service.NormalizedName == request.serviceNormalizedName);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var size = Math.Min(request.pageSize, _maxPageSize);
        var page = Math.Max(1, request.pageNo);
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);

        var items = await query
            .Skip(size * (page - 1))
            .Take(size)
            .ToListAsync(cancellationToken);

        return ResponseData<ListModel<Product>>.Success(new ListModel<Product>
        {
            Items = items,
            CurrentPage = page,
            TotalPages = totalPages
        });
    }
}
