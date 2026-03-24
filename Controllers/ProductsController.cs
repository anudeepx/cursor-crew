using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Products;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] string? search)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue && brandId.Value > 0)
        {
            query = query.Where(p => p.SellerId == brandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(normalizedSearch) ||
                p.Category.Name.ToLower().Contains(normalizedSearch) ||
                p.Seller.Name.ToLower().Contains(normalizedSearch));
        }

        var response = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = new CategoryDto { Id = p.CategoryId, Name = p.Category.Name },
                Seller = new SellerDto
                {
                    Id = p.SellerId,
                    Name = p.Seller.Name,
                    Email = p.Seller.Email,
                    Phone = p.Seller.Phone,
                    Address = p.Seller.Address
                }
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ProductDto>>.Ok(response));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = new CategoryDto { Id = p.CategoryId, Name = p.Category.Name },
                Seller = new SellerDto
                {
                    Id = p.SellerId,
                    Name = p.Seller.Name,
                    Email = p.Seller.Email,
                    Phone = p.Seller.Phone,
                    Address = p.Seller.Address
                }
            })
            .FirstOrDefaultAsync();

        if (product is null)
        {
            return NotFound(ApiResponse<ProductDto>.Fail("Product not found."));
        }

        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(UpsertProductRequestDto request)
    {
        var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(ApiResponse<ProductDto>.Fail("Category not found."));
        }

        var brandExists = await _dbContext.Sellers.AnyAsync(s => s.Id == request.SellerId);
        if (!brandExists)
        {
            return BadRequest(ApiResponse<ProductDto>.Fail("Brand not found."));
        }

        var product = new Models.Product
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            SellerId = request.SellerId
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .FirstAsync(p => p.Id == product.Id);

        return Ok(ApiResponse<ProductDto>.Ok(MapProduct(saved), "Product created."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, UpsertProductRequestDto request)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound(ApiResponse<ProductDto>.Fail("Product not found."));
        }

        var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(ApiResponse<ProductDto>.Fail("Category not found."));
        }

        var brandExists = await _dbContext.Sellers.AnyAsync(s => s.Id == request.SellerId);
        if (!brandExists)
        {
            return BadRequest(ApiResponse<ProductDto>.Fail("Brand not found."));
        }

        product.Name = request.Name;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.SellerId = request.SellerId;
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .FirstAsync(p => p.Id == product.Id);

        return Ok(ApiResponse<ProductDto>.Ok(MapProduct(saved), "Product updated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteProduct(int id)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound(ApiResponse<string>.Fail("Product not found."));
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Deleted", "Product deleted."));
    }

    private static ProductDto MapProduct(Models.Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        Stock = product.Stock,
        Category = new CategoryDto { Id = product.CategoryId, Name = product.Category.Name },
        Seller = new SellerDto
        {
            Id = product.SellerId,
            Name = product.Seller.Name,
            Email = product.Seller.Email,
            Phone = product.Seller.Phone,
            Address = product.Seller.Address
        }
    };
}
