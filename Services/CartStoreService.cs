namespace RetailOrderingWebsite.Services;

public class CartStoreService
{
    private readonly Dictionary<int, Dictionary<int, int>> _cartsByUserId = new();
    private readonly object _syncRoot = new();

    public IReadOnlyDictionary<int, int> GetCart(int userId)
    {
        lock (_syncRoot)
        {
            if (!_cartsByUserId.TryGetValue(userId, out var cart))
            {
                return new Dictionary<int, int>();
            }

            return new Dictionary<int, int>(cart);
        }
    }

    public void AddOrIncrement(int userId, int productId, int quantity)
    {
        lock (_syncRoot)
        {
            var cart = GetOrCreateCart(userId);
            cart[productId] = cart.GetValueOrDefault(productId) + quantity;
        }
    }

    public void SetQuantity(int userId, int productId, int quantity)
    {
        lock (_syncRoot)
        {
            var cart = GetOrCreateCart(userId);
            if (quantity <= 0)
            {
                cart.Remove(productId);
                return;
            }

            cart[productId] = quantity;
        }
    }

    public void Remove(int userId, int productId)
    {
        lock (_syncRoot)
        {
            if (_cartsByUserId.TryGetValue(userId, out var cart))
            {
                cart.Remove(productId);
            }
        }
    }

    public void Clear(int userId)
    {
        lock (_syncRoot)
        {
            _cartsByUserId.Remove(userId);
        }
    }

    private Dictionary<int, int> GetOrCreateCart(int userId)
    {
        if (!_cartsByUserId.TryGetValue(userId, out var cart))
        {
            cart = new Dictionary<int, int>();
            _cartsByUserId[userId] = cart;
        }

        return cart;
    }
}
