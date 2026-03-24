using RetailOrderingWebsite.DTOs.Payments;

namespace RetailOrderingWebsite.Services;

public class PaymentStoreService
{
    private readonly Dictionary<int, PaymentResponseDto> _paymentsByOrder = new();
    private readonly object _syncRoot = new();

    public PaymentResponseDto Create(int orderId)
    {
        lock (_syncRoot)
        {
            var payment = new PaymentResponseDto
            {
                OrderId = orderId,
                Status = "Pending",
                TransactionId = $"TXN-{Guid.NewGuid():N}"[..18],
                UpdatedAt = DateTime.UtcNow
            };
            _paymentsByOrder[orderId] = payment;
            return payment;
        }
    }

    public PaymentResponseDto? Get(int orderId)
    {
        lock (_syncRoot)
        {
            return _paymentsByOrder.TryGetValue(orderId, out var payment)
                ? Clone(payment)
                : null;
        }
    }

    public PaymentResponseDto Verify(int orderId, string transactionId)
    {
        lock (_syncRoot)
        {
            var payment = _paymentsByOrder.GetValueOrDefault(orderId) ?? new PaymentResponseDto
            {
                OrderId = orderId,
                Status = "Pending",
                TransactionId = transactionId,
                UpdatedAt = DateTime.UtcNow
            };

            payment.Status = "Verified";
            payment.TransactionId = transactionId;
            payment.UpdatedAt = DateTime.UtcNow;
            _paymentsByOrder[orderId] = payment;
            return Clone(payment);
        }
    }

    private static PaymentResponseDto Clone(PaymentResponseDto payment) => new()
    {
        OrderId = payment.OrderId,
        Status = payment.Status,
        TransactionId = payment.TransactionId,
        UpdatedAt = payment.UpdatedAt
    };
}
