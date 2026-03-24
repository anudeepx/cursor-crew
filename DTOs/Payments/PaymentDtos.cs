namespace RetailOrderingWebsite.DTOs.Payments;

public class PaymentCreateRequestDto
{
    public int OrderId { get; set; }
}

public class PaymentVerifyRequestDto
{
    public int OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
}

public class PaymentResponseDto
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
