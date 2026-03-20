namespace SpiritShop.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public double AlcoholPercentage { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string? ImageFileName { get; set; }
    public string? ImageContentType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsAvailable => StockQuantity > 0;

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");
        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
