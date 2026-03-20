using MediatR;
using SpiritShop.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace SpiritShop.Application.Commands;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductDto>;

public record UpdateProductCommand(int Id, UpdateProductDto Dto) : IRequest<ProductDto?>;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public record UploadProductImageCommand(int ProductId, IFormFile File) : IRequest<string>;
