using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Cart;
using RetailOrderingWebsite.Services;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly CartStoreService _cartStoreService;

    public CartController(AppDbContext dbContext, CartStoreService cartStoreService)
    {
        _dbContext = dbContext;
        _cartStoreService = cartStoreService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> GetCart()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<CartResponseDto>.Fail("Invalid token."));
        }

        var cart = await BuildCart(userId.Value);
        return Ok(ApiResponse<CartResponseDto>.Ok(cart));
    }

    [HttpPost("add")]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> Add(AddCartItemRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<CartResponseDto>.Fail("Invalid token."));
        }

        var exists = await _dbContext.Products.AnyAsync(p => p.Id == request.ProductId);
        if (!exists)
        {
            return BadRequest(ApiResponse<CartResponseDto>.Fail("Product not found."));
        }

        _cartStoreService.AddOrIncrement(userId.Value, request.ProductId, request.Quantity);
        var cart = await BuildCart(userId.Value);
        return Ok(ApiResponse<CartResponseDto>.Ok(cart, "Cart updated."));
    }

    [HttpPut("update")]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> Update(UpdateCartItemRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<CartResponseDto>.Fail("Invalid token."));
        }

        _cartStoreService.SetQuantity(userId.Value, request.ProductId, request.Quantity);
        var cart = await BuildCart(userId.Value);
        return Ok(ApiResponse<CartResponseDto>.Ok(cart, "Cart updated."));
    }

    [HttpDelete("remove/{productId:int}")]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> Remove(int productId)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<CartResponseDto>.Fail("Invalid token."));
        }

        _cartStoreService.Remove(userId.Value, productId);
        var cart = await BuildCart(userId.Value);
        return Ok(ApiResponse<CartResponseDto>.Ok(cart, "Item removed."));
    }

    [HttpDelete("clear")]
    public ActionResult<ApiResponse<string>> Clear()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid token."));
        }

        _cartStoreService.Clear(userId.Value);
        return Ok(ApiResponse<string>.Ok("Cleared", "Cart cleared."));
    }

    private async Task<CartResponseDto> BuildCart(int userId)
    {
        var cartMap = _cartStoreService.GetCart(userId);
        var productIds = cartMap.Keys.ToList();
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var items = products.Select(product =>
        {
            var quantity = cartMap.GetValueOrDefault(product.Id);
            return new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                LineTotal = quantity * product.Price
            };
        }).ToList();

        return new CartResponseDto
        {
            Items = items,
            TotalAmount = items.Sum(i => i.LineTotal)
        };
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
