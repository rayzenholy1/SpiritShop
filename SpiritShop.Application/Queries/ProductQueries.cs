using MediatR;
using SpiritShop.Application.DTOs;

namespace SpiritShop.Application.Queries;

public record GetAllProductsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Category = null,
    decimal? MaxPrice = null
) : IRequest<PagedResult<ProductDto>>;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
