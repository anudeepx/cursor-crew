namespace RetailOrderingWebsite.Services;

public static class OrderStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Preparing = "Preparing";
    public const string OutForDelivery = "OutForDelivery";
    public const string Delivered = "Delivered";
    public static readonly HashSet<string> Allowed =
    [
        Pending,
        Confirmed,
        Preparing,
        OutForDelivery,
        Delivered
    ];
}
