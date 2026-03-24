namespace RetailOrderingWebsite.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int SellerId { get; set; }
    public Seller Seller { get; set; } = null!;

    public int Stock { get; set; }
}
