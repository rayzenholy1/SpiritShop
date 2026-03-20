using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SpiritShop.Application.Commands;
using SpiritShop.Application.DTOs;
using SpiritShop.Application.Interfaces;
using SpiritShop.Application.Queries;
using SpiritShop.Domain.Entities;
using SpiritShop.Infrastructure.Data;
using Xunit;

namespace SpiritShop.Tests.Unit;

public class GetProductByIdQueryHandlerTests
{
    private static AppDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ReturnsProductDto()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateInMemoryContext(dbName);

        var category = new Category { Id = 1, Name = "Whiskey" };
        var product = new Product
        {
            Id = 1,
            Name = "Jack Daniel's",
            Price = 599.99m,
            AlcoholPercentage = 40,
            StockQuantity = 10,
            CategoryId = 1,
            Category = category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new GetProductByIdQueryHandler(context);

        var result = await handler.Handle(new GetProductByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Jack Daniel's");
        result.Price.Should().Be(599.99m);
        result.CategoryName.Should().Be("Whiskey");
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ReturnsNull()
    {
        await using var context = CreateInMemoryContext(Guid.NewGuid().ToString());
        var handler = new GetProductByIdQueryHandler(context);

        var result = await handler.Handle(new GetProductByIdQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidProduct_ReturnsCreatedDto()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateInMemoryContext(dbName);

        context.Categories.Add(new Category { Id = 1, Name = "Vodka" });
        await context.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(context);
        var dto = new CreateProductDto
        {
            Name = "Nemiroff Original",
            Description = "Ukrainian vodka",
            Price = 249.00m,
            StockQuantity = 50,
            AlcoholPercentage = 40,
            CategoryId = 1
        };

        var result = await handler.Handle(new CreateProductCommand(dto), CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Nemiroff Original");
        result.Price.Should().Be(249.00m);
        result.IsAvailable.Should().BeTrue("stock is 50");

        var savedProduct = await context.Products.FindAsync(result.Id);
        savedProduct.Should().NotBeNull();
    }

    private static AppDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }
}

public class DeleteProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_NonExistingProduct_ReturnsFalse()
    {
        var mockContext = new Mock<IAppDbContext>();

        var dbName = Guid.NewGuid().ToString();
        var realContext = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName).Options);

        mockContext.Setup(c => c.Products).Returns(realContext.Products);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(0);

        var handler = new DeleteProductCommandHandler(mockContext.Object);

        var result = await handler.Handle(new DeleteProductCommand(9999), CancellationToken.None);

        result.Should().BeFalse();

        mockContext.Verify(
            c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "SaveChangesAsync should not be called when product doesn't exist");
    }
}

public class ProductEntityTests
{
    [Fact]
    public void UpdateStock_NegativeValue_ThrowsDomainException()
    {
        var product = new Product { StockQuantity = 10 };

        var act = () => product.UpdateStock(-1);

        act.Should().Throw<DomainException>()
           .WithMessage("*negative*");
    }

    [Theory]
    [InlineData(0, false)]  
    [InlineData(1, true)]    
    [InlineData(100, true)]  
    public void IsAvailable_ReturnsCorrectValue(int stock, bool expected)
    {
        var product = new Product { StockQuantity = stock };

        product.IsAvailable.Should().Be(expected);
    }
}
