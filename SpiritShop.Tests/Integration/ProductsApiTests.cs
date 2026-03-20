using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpiritShop.Application.DTOs;
using SpiritShop.Infrastructure.Data;
using Xunit;

namespace SpiritShop.Tests.Integration;

public class SpiritShopWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid()));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private static void SeedTestData(AppDbContext context)
    {
        context.Categories.AddRange(
            new Domain.Entities.Category { Id = 1, Name = "Whiskey" },
            new Domain.Entities.Category { Id = 2, Name = "Vodka" }
        );
        context.Products.AddRange(
            new Domain.Entities.Product
            {
                Id = 1, Name = "Test Whiskey", Price = 500m,
                AlcoholPercentage = 40, StockQuantity = 10, CategoryId = 1,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            }
        );
        context.SaveChanges();
    }
}

public class ProductsApiIntegrationTests : IClassFixture<SpiritShopWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiIntegrationTests(SpiritShopWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_Products_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GET_Product_ById_ReturnsProduct()
    {
        var response = await _client.GetAsync("/api/products/1");
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(product);
        Assert.Equal("Test Whiskey", product.Name);
    }

    [Fact]
    public async Task GET_Product_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/products/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_Product_WithoutAuth_Returns401()
    {
        var dto = new CreateProductDto
        {
            Name = "New Product",
            Price = 100m,
            AlcoholPercentage = 40,
            StockQuantity = 5,
            CategoryId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/products", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
