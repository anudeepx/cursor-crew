namespace RetailOrderingWebsite.DTOs.Products;

public class UpsertProductRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public int SellerId { get; set; }
}
