using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Brands;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public BrandsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BrandDto>>>> GetBrands()
    {
        var brands = await _dbContext.Sellers
            .OrderBy(s => s.Id)
            .Select(s => new BrandDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address
            })
            .ToListAsync();

        return Ok(ApiResponse<List<BrandDto>>.Ok(brands));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> CreateBrand(UpsertBrandRequestDto request)
    {
        var seller = new Models.Seller
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
        };

        _dbContext.Sellers.Add(seller);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<BrandDto>.Ok(MapBrand(seller), "Brand created."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> UpdateBrand(int id, UpsertBrandRequestDto request)
    {
        var seller = await _dbContext.Sellers.FirstOrDefaultAsync(s => s.Id == id);
        if (seller is null)
        {
            return NotFound(ApiResponse<BrandDto>.Fail("Brand not found."));
        }

        seller.Name = request.Name;
        seller.Email = request.Email;
        seller.Phone = request.Phone;
        seller.Address = request.Address;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<BrandDto>.Ok(MapBrand(seller), "Brand updated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteBrand(int id)
    {
        var seller = await _dbContext.Sellers.FirstOrDefaultAsync(s => s.Id == id);
        if (seller is null)
        {
            return NotFound(ApiResponse<string>.Fail("Brand not found."));
        }

        var inUse = await _dbContext.Products.AnyAsync(p => p.SellerId == id);
        if (inUse)
        {
            return BadRequest(ApiResponse<string>.Fail("Cannot delete brand with products."));
        }

        _dbContext.Sellers.Remove(seller);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Deleted", "Brand deleted."));
    }

    private static BrandDto MapBrand(Models.Seller seller) => new()
    {
        Id = seller.Id,
        Name = seller.Name,
        Email = seller.Email,
        Phone = seller.Phone,
        Address = seller.Address
    };
}
