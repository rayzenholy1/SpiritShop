using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SpiritShop.Domain.Entities;

namespace SpiritShop.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DatabaseFacade Database { get; }
}

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string fileKey, CancellationToken cancellationToken = default);

    string GetFileUrl(string fileKey);

    Task DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default);
}
public interface IJwtTokenService
{
    string GenerateToken(string userId, string email, string fullName, IList<string> roles);
}
