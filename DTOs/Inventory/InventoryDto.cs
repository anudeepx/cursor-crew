namespace RetailOrderingWebsite.DTOs.Inventory;

public class InventoryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}

public class UpdateInventoryRequestDto
{
    public int StockQuantity { get; set; }
}
