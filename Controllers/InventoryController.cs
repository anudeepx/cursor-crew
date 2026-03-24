using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Inventory;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public InventoryController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<InventoryDto>>>> GetInventory()
    {
        var items = await _dbContext.Products
            .OrderBy(p => p.Name)
            .Select(p => new InventoryDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                StockQuantity = p.Stock
            })
            .ToListAsync();

        return Ok(ApiResponse<List<InventoryDto>>.Ok(items));
    }

    [HttpPut("{productId:int}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> UpdateInventory(int productId, UpdateInventoryRequestDto request)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null)
        {
            return NotFound(ApiResponse<InventoryDto>.Fail("Product not found."));
        }

        product.Stock = request.StockQuantity;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<InventoryDto>.Ok(new InventoryDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            StockQuantity = product.Stock
        }, "Inventory updated."));
    }
}
