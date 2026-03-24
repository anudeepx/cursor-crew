namespace RetailOrderingWebsite.DTOs.Cart;

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class CartResponseDto
{
    public List<CartItemDto> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
}

public class AddCartItemRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
