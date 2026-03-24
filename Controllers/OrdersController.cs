using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Orders;
using RetailOrderingWebsite.Services;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly CartStoreService _cartStoreService;

    public OrdersController(AppDbContext dbContext, CartStoreService cartStoreService)
    {
        _dbContext = dbContext;
        _cartStoreService = cartStoreService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> PlaceOrder(PlaceOrderRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<OrderResponseDto>.Fail("Invalid token."));
        }

        var items = request.Items.Where(i => i.Quantity > 0).ToList();
        if (items.Count == 0)
        {
            return BadRequest(ApiResponse<OrderResponseDto>.Fail("Order must contain at least one valid item."));
        }

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            return BadRequest(ApiResponse<OrderResponseDto>.Fail("One or more products were not found."));
        }

        decimal total = 0m;
        var orderItems = new List<Models.OrderItem>();

        foreach (var item in items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            if (product.Stock < item.Quantity)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.Fail($"Insufficient stock for {product.Name}."));
            }

            product.Stock -= item.Quantity;
            total += product.Price * item.Quantity;

            orderItems.Add(new Models.OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                Price = product.Price
            });
        }

        var order = new Models.Order
        {
            UserId = userId.Value,
            TotalAmount = total,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _cartStoreService.Clear(userId.Value);

        var response = MapOrder(order, products.ToDictionary(x => x.Id, x => x.Name));
        return Ok(ApiResponse<OrderResponseDto>.Ok(response, "Order placed successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetMyOrders()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<List<OrderResponseDto>>.Fail("Invalid token."));
        }

        var orders = await ProjectOrderResponse(
                _dbContext.Orders
                    .AsNoTracking()
                    .Where(o => o.UserId == userId.Value))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<OrderResponseDto>>.Ok(orders));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetOrder(int id)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<OrderResponseDto>.Fail("Invalid token."));
        }

        var isAdmin = User.IsInRole("Admin");
        var orderMeta = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new { o.Id, o.UserId })
            .FirstOrDefaultAsync();

        if (orderMeta is null)
        {
            return NotFound(ApiResponse<OrderResponseDto>.Fail("Order not found."));
        }

        if (!isAdmin && orderMeta.UserId != userId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<OrderResponseDto>.Fail("Access denied."));
        }

        var order = await ProjectOrderResponse(
                _dbContext.Orders
                    .AsNoTracking()
                    .Where(o => o.Id == id))
            .FirstAsync();

        return Ok(ApiResponse<OrderResponseDto>.Ok(order));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetAllOrders()
    {
        var orders = await ProjectOrderResponse(_dbContext.Orders.AsNoTracking())
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<OrderResponseDto>>.Ok(orders));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateStatus(int id, UpdateOrderStatusRequestDto request)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            return NotFound(ApiResponse<OrderResponseDto>.Fail("Order not found."));
        }

        order.Status = request.Status;
        await _dbContext.SaveChangesAsync();

        var updated = await LoadOrdersQuery().FirstAsync(o => o.Id == id);
        return Ok(ApiResponse<OrderResponseDto>.Ok(MapOrder(updated), "Order status updated."));
    }

    [HttpGet("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<string>>> GetOrderStatus(int id)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid token."));
        }

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            return NotFound(ApiResponse<string>.Fail("Order not found."));
        }

        if (!User.IsInRole("Admin") && order.UserId != userId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access denied."));
        }

        return Ok(ApiResponse<string>.Ok(order.Status));
    }

    [HttpPut("{id:int}/track")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> TrackOrder(int id, UpdateOrderStatusRequestDto request)
    {
        return await UpdateStatus(id, request);
    }

    private static IQueryable<OrderResponseDto> ProjectOrderResponse(IQueryable<Models.Order> query)
    {
        return query.Select(order => new OrderResponseDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        });
    }

    private IQueryable<Models.Order> LoadOrdersQuery()
    {
        return _dbContext.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product);
    }

    private static OrderResponseDto MapOrder(Models.Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };
    }

    private static OrderResponseDto MapOrder(Models.Order order, Dictionary<int, string> productNames)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                ProductId = oi.ProductId,
                ProductName = productNames.GetValueOrDefault(oi.ProductId, "Product"),
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
