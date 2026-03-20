using MediatR;
using Microsoft.EntityFrameworkCore;
using SpiritShop.Application.DTOs;
using SpiritShop.Application.Interfaces;

namespace SpiritShop.Application.Queries;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IAppDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public GetAllProductsQueryHandler(IAppDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<PagedResult<ProductDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(p => p.Category!.Name.ToLower() == request.Category.ToLower());

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                AlcoholPercentage = p.AlcoholPercentage,
                IsAvailable = p.StockQuantity > 0,
                CategoryName = p.Category!.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.ImageUrl = _fileStorage.GetFileUrl(item.Id.ToString());
        }

        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IAppDbContext _context;

    public GetProductByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            AlcoholPercentage = product.AlcoholPercentage,
            IsAvailable = product.IsAvailable,
            CategoryName = product.Category?.Name ?? "",
            CreatedAt = product.CreatedAt
        };
    }
}
