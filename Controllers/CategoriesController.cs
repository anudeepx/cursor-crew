using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Products;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CategoriesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
    {
        var categories = await _dbContext.Categories
            .OrderBy(c => c.Id)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();

        return Ok(ApiResponse<List<CategoryDto>>.Ok(categories));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory(UpsertCategoryRequestDto request)
    {
        var category = new Models.Category { Name = request.Name };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<CategoryDto>.Ok(new CategoryDto { Id = category.Id, Name = category.Name }, "Category created."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, UpsertCategoryRequestDto request)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound(ApiResponse<CategoryDto>.Fail("Category not found."));
        }

        category.Name = request.Name;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<CategoryDto>.Ok(new CategoryDto { Id = category.Id, Name = category.Name }, "Category updated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(int id)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound(ApiResponse<string>.Fail("Category not found."));
        }

        var inUse = await _dbContext.Products.AnyAsync(p => p.CategoryId == id);
        if (inUse)
        {
            return BadRequest(ApiResponse<string>.Fail("Cannot delete category with products."));
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Deleted", "Category deleted."));
    }
}
