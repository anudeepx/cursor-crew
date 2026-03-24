using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailOrderingWebsite.Data;
using RetailOrderingWebsite.DTOs;
using RetailOrderingWebsite.DTOs.Users;
using RetailOrderingWebsite.Models;
using RetailOrderingWebsite.Services;

namespace RetailOrderingWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly UserRoleService _userRoleService;

    public UsersController(AppDbContext dbContext, UserRoleService userRoleService)
    {
        _dbContext = dbContext;
        _userRoleService = userRoleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers()
    {
        var users = await _dbContext.Users.OrderBy(u => u.Id).ToListAsync();
        return Ok(ApiResponse<List<UserDto>>.Ok(users.Select(MapUser).ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));
        }

        return Ok(ApiResponse<UserDto>.Ok(MapUser(user)));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, UpdateUserRequestDto request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));
        }

        var emailTaken = await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != id);
        if (emailTaken)
        {
            return BadRequest(ApiResponse<UserDto>.Fail("Email is already in use."));
        }

        user.Name = request.Name;
        user.Email = request.Email;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<UserDto>.Ok(MapUser(user), "User updated."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(ApiResponse<string>.Fail("User not found."));
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Deleted", "User deleted."));
    }

    private UserDto MapUser(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = _userRoleService.GetRoleForUser(user)
        };
    }
}
