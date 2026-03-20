using MediatR;
using Microsoft.EntityFrameworkCore;
using SpiritShop.Application.DTOs;
using SpiritShop.Application.Interfaces;
using SpiritShop.Domain.Entities;

namespace SpiritShop.Application.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IAppDbContext _context;

    public CreateProductCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            AlcoholPercentage = dto.AlcoholPercentage,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Products.Entry(product)
            .Reference(p => p.Category)
            .LoadAsync(cancellationToken);

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

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly IAppDbContext _context;

    public UpdateProductCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null) return null;

        var dto = request.Dto;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.AlcoholPercentage = dto.AlcoholPercentage;
        product.CategoryId = dto.CategoryId;
        product.UpdateStock(dto.StockQuantity);
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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


public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteProductCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IFileStorageService _fileStorage;

    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public UploadProductImageCommandHandler(IAppDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<string> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;

        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File exceeds 5 MB limit. Actual size: {file.Length / 1024 / 1024} MB.");

        if (!AllowedContentTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException($"Invalid file type '{file.ContentType}'. Allowed: JPEG, PNG, WebP.");

        var fileName = await _fileStorage.SaveFileAsync(file, request.ProductId.ToString(), cancellationToken);

        var product = await _context.Products.FindAsync([request.ProductId], cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        product.ImageFileName = fileName;
        product.ImageContentType = file.ContentType;
        await _context.SaveChangesAsync(cancellationToken);

        return _fileStorage.GetFileUrl(request.ProductId.ToString());
    }
}
