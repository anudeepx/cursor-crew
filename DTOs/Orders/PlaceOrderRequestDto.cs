namespace RetailOrderingWebsite.DTOs.Orders;

public class PlaceOrderRequestDto
{
    public List<OrderItemRequestDto> Items { get; set; } = [];
}

public class OrderItemRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
